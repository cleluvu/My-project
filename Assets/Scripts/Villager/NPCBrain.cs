using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

[RequireComponent(typeof(NPCActionController))]
[RequireComponent(typeof(NPCCoreSystems))]
public class NPCBrain : Agent
{
    [Header("MARL Hyperparameters")]
    [Range(0f, 1f)]
    public float cooperativeFactor = 0.3f;

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
        // Tách bạch 2 luồng phần thưởng theo mô hình toán học
        float stepIndividualReward = -0.0005f; // Phạt thời gian mặc định
        float stepSocialReward = 0f;

        // ==========================================
        // 1. PHÁ VỠ LỊCH TRÌNH CỨNG (Soft-Constraints)
        // ==========================================
        if (coreSystems.currentTask != null)
        {
            // Quy ước: Khung giờ có lệnh Wander được xem là "Giờ Tự Do"
            bool isFreeTime = (coreSystems.currentTask.action == NPCAction.Wander);

            if (currentAction == coreSystems.currentTask.action)
            {
                stepIndividualReward += 0.01f; // Tuân thủ lịch trình
            }
            else if (isFreeTime && (currentAction == NPCAction.Talk || currentAction == NPCAction.Trade))
            {
                // Thưởng sáng kiến: Thay vì đi lang thang vô mục đích, AI biết dùng thời gian rảnh để xây dựng quan hệ
                stepIndividualReward += 0.005f; 
            }
            else
            {
                stepIndividualReward -= 0.005f; // Bỏ bê công việc (Rest/Work) sẽ bị phạt
            }
        }

        // ==========================================
        // 2. TƯƠNG TÁC & HÀM PHẦN THƯỞNG HỢP TÁC (Cooperative Reward Shaping)
        // ==========================================
        if (currentAction == NPCAction.Trade || currentAction == NPCAction.Talk)
        {
            if (Time.time - lastInteractionTime < INTERACTION_COOLDOWN)
            {
                stepIndividualReward -= 0.01f; // Phạt spam
            }
            else
            {
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
                                    // Cập nhật trạng thái môi trường
                                    coreSystems.currentNeeds.socialNeed = Mathf.Clamp(coreSystems.currentNeeds.socialNeed + 20f, 0, 100);
                                    otherNPC.currentNeeds.socialNeed = Mathf.Clamp(otherNPC.currentNeeds.socialNeed + 20f, 0, 100);

                                    Relationship myRel = coreSystems.GetRelationship(otherNPC.gameObject);
                                    Relationship theirRel = otherNPC.GetRelationship(this.gameObject);
                                    
                                    float boost = (coreSystems.personality == PersonalityType.Friendly) ? 2f : 1f;
                                    myRel.friendship = Mathf.Clamp(myRel.friendship + boost, -100, 100);
                                    theirRel.friendship = Mathf.Clamp(theirRel.friendship + boost, -100, 100);

                                    // Tích lũy điểm vào các bể phần thưởng tương ứng
                                    if (coreSystems.currentNeeds.socialNeed < 50f) 
                                        stepIndividualReward += 0.02f; // Lợi ích cá nhân (giải tỏa cô đơn)
                                    
                                    if (otherNPC.currentNeeds.socialNeed < 50f) 
                                    {
                                        stepSocialReward += 0.04f; // Lợi ích xã hội (giúp đỡ người khác)
                                        
                                        // Thưởng đồng bộ cực kỳ quan trọng cho MARL
                                        NPCBrain otherBrain = otherNPC.GetComponent<NPCBrain>();
                                        if (otherBrain != null) otherBrain.AddReward(0.04f);
                                    }
                                    
                                    interactedSuccessfully = true;
                                    break; 
                                }
                                else { continue; }
                            }
                            else if (currentAction == NPCAction.Trade)
                            {
                                Relationship theirRelWithMe = otherNPC.GetRelationship(this.gameObject);
                                if (theirRelWithMe.trust >= 30f)
                                {
                                    stepSocialReward += 0.05f; // Giao thương thúc đẩy nền kinh tế chung
                                    interactedSuccessfully = true;
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
                                stepIndividualReward += 0.05f; // Trade với người chơi mang lại tài nguyên cá nhân
                                interactedSuccessfully = true;
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
                            stepIndividualReward += 0.02f;
                            interactedSuccessfully = true;
                            break;
                        }
                    }
                }

                if (!interactedSuccessfully)
                {
                    stepIndividualReward -= 0.04f; // Phạt lãng phí action
                }
            }
        }

        // ==========================================
        // 3. TÍNH TOÁN EXPECTED RETURN CUỐI CÙNG
        // R_total = (1 - lambda) * R_ind + lambda * R_soc
        // ==========================================
        float finalReward = (1f - cooperativeFactor) * stepIndividualReward + (cooperativeFactor) * stepSocialReward;
        
        if (finalReward != 0f)
        {
            AddReward(finalReward);
        }
    }

    private void CheckEpisodeEnd()
    {
        // Thất bại
        if (coreSystems.currentNeeds.energy <= 0 || coreSystems.currentNeeds.hunger <= 0)
        {
            Debug.Log($"{gameObject.name} đã kiệt sức!");
            AddReward(-1.0f);
            // EndEpisode();
        }

        // Thành công sống qua ngày
        if (coreSystems.timeManager != null && 
            coreSystems.timeManager.allTimeFromBegin - startTime >= coreSystems.timeManager.dayDuration)
        {
            Debug.Log($"{gameObject.name} sống sót qua 1 ngày!");
            AddReward(1.0f); 
            // EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = (int)NPCAction.Wander; 
    }
}