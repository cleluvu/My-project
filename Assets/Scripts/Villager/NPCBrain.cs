using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

[RequireComponent(typeof(NPCActionController))]
[RequireComponent(typeof(NPCCoreSystems))]
public class NPCBrain : Agent
{
    private NPCActionController actionController;
    private NPCCoreSystems coreSystems;
    
    private Vector3 startingPosition;
    private float startTime;

    private float lastInteractionTime = -1f;
    private const float INTERACTION_COOLDOWN = 0.5f;

    public override void Initialize()
    {
        actionController = GetComponent<NPCActionController>();
        coreSystems = GetComponent<NPCCoreSystems>();
        startingPosition = transform.position;
    }

    public override void OnEpisodeBegin()
    {
        transform.position = startingPosition;
        actionController.StopMovement();
        
        coreSystems.currentNeeds.hunger = 100f;
        coreSystems.currentNeeds.energy = 100f;
        coreSystems.currentNeeds.socialNeed = 100f;

        if (coreSystems.timeManager != null)
        {
            startTime = coreSystems.timeManager.allTimeFromBegin;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Thời gian
        sensor.AddObservation(coreSystems.currentHour / 24f);

        // Nhu cầu cơ bản
        sensor.AddObservation(coreSystems.currentNeeds.hunger / 100f);
        sensor.AddObservation(coreSystems.currentNeeds.energy / 100f);
        sensor.AddObservation(coreSystems.currentNeeds.socialNeed / 100f);

        // Cảm xúc
        sensor.AddObservation((float)coreSystems.currentEmotion / 4f);

        // Nhiệm vụ hiện tại
        if (coreSystems.currentTask != null)
        {
            sensor.AddOneHotObservation((int)coreSystems.currentTask.action, 7); 
        }
        else
        {
            sensor.AddOneHotObservation((int)NPCAction.Wander, 7); 
        }

        // Lượt giao tiếp với người chơi
        sensor.AddObservation(coreSystems.CanInteractWithPlayerToday() ? 1f : 0f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int actionValue = actions.DiscreteActions[0];
        NPCAction decidedAction = (NPCAction)actionValue;

        actionController.ExecuteAction(decidedAction);
        CalculateRewards(decidedAction);
        CheckEpisodeEnd(); 
    }

    private void CalculateRewards(NPCAction currentAction)
    {
        // Phạt nhẹ thời gian để tránh lười biếng
        AddReward(-0.0005f);

        // Kiểm tra lịch trình
        if (coreSystems.currentTask != null)
        {
            if (currentAction == coreSystems.currentTask.action)
            {
                AddReward(0.01f);
            }
            else
            {
                AddReward(-0.005f);
            }
        }

        // Tương tác
        if (currentAction == NPCAction.Trade || currentAction == NPCAction.Talk)
        {
            if (Time.time - lastInteractionTime < INTERACTION_COOLDOWN)
            {
                AddReward(-0.01f); 
                return;
            }

            lastInteractionTime = Time.time;

            Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, 3f);
            bool interactedSuccessfully = false;

            foreach (var col in nearbyColliders)
            {
                if (col.gameObject == this.gameObject) continue;

                if (col.CompareTag("NPC"))
                {
                    NPCCoreSystems otherNPC = col.GetComponent<NPCCoreSystems>();
                    if (otherNPC != null)
                    {
                        if (currentAction == NPCAction.Talk)
                        {
                            if (coreSystems.currentNeeds.socialNeed < 95f || otherNPC.currentNeeds.socialNeed < 95f)
                            {
                                coreSystems.currentNeeds.socialNeed = Mathf.Clamp(coreSystems.currentNeeds.socialNeed + 20f, 0, 100);
                                otherNPC.currentNeeds.socialNeed = Mathf.Clamp(otherNPC.currentNeeds.socialNeed + 20f, 0, 100);

                                Relationship myRel = coreSystems.GetRelationship(otherNPC.gameObject);
                                Relationship theirRel = otherNPC.GetRelationship(this.gameObject);
                                
                                float boost = (coreSystems.personality == PersonalityType.Friendly) ? 2f : 1f;
                                myRel.friendship = Mathf.Clamp(myRel.friendship + boost, -100, 100);
                                theirRel.friendship = Mathf.Clamp(theirRel.friendship + boost, -100, 100);

                                if (coreSystems.currentNeeds.socialNeed < 50f) 
                                {
                                    AddReward(0.02f);
                                }
                                
                                if (otherNPC.currentNeeds.socialNeed < 50f) 
                                {
                                    AddReward(0.04f); 
                                    NPCBrain otherBrain = otherNPC.GetComponent<NPCBrain>();
                                    if (otherBrain != null) otherBrain.AddReward(0.04f);
                                }
                                
                                interactedSuccessfully = true;
                                break; 
                            }
                            else
                            {
                                continue; 
                            }
                        }
                        else if (currentAction == NPCAction.Trade)
                        {
                            Relationship theirRelWithMe = otherNPC.GetRelationship(this.gameObject);
                            
                            if (theirRelWithMe.trust >= 30f)
                            {
                                interactedSuccessfully = true;
                                AddReward(0.05f);
                                break;
                            }
                            else
                            {
                                interactedSuccessfully = false;
                                continue; 
                            }
                        }
                    }
                }
                else if (col.CompareTag("Player") && coreSystems.CanInteractWithPlayerToday())
                {
                    if (currentAction == NPCAction.Trade)
                    {
                        if (coreSystems.playerRelationship.trust >= 30f)
                        {
                            interactedSuccessfully = true;
                            AddReward(0.05f);
                            break;
                        }
                        else
                        {
                            interactedSuccessfully = false;
                            continue; 
                        }
                    }
                    else if (currentAction == NPCAction.Talk)
                    {
                        AddReward(0.02f);
                        interactedSuccessfully = true;
                        break;
                    }
                }
            }

            if (!interactedSuccessfully)
            {
                AddReward(-0.04f); 
            }
        }

        // if (coreSystems.currentNeeds.energy < 10) AddReward(-0.01f);
        // if (coreSystems.currentNeeds.hunger < 10) AddReward(-0.01f);
    }

    private void CheckEpisodeEnd()
    {
        // Thất bại
        if (coreSystems.currentNeeds.energy <= 0 || coreSystems.currentNeeds.hunger <= 0)
        {
            Debug.Log($"{gameObject.name} đã kiệt sức!");
            AddReward(-1.0f);
            EndEpisode();
        }

        // Thành công sống qua ngày
        if (coreSystems.timeManager != null && 
            coreSystems.timeManager.allTimeFromBegin - startTime >= coreSystems.timeManager.dayDuration)
        {
            Debug.Log($"{gameObject.name} sống sót qua 1 ngày!");
            AddReward(1.0f); 
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = (int)NPCAction.Wander; 
    }
}