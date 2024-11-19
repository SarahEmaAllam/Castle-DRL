// using MLAgents;
// using System.Collections;
// using System.Collections.Generic;
// using Unity.MLAgents;
// using UnityEngine;
//
// public class CastleAcademy : Academy
// {
//     private CastleArea[] areas;
//     private int m_ResetTimer;
//     public int MaxEnvironmentSteps = 1000;
//     private int MaxTraining = 5000;
//
//     public int targetCount = 2;
//     // private bool m_Initialized;
//
//
//     /// <summary>
//     /// Reset the academy
//     /// </summary>
//     ///
//     public override void InitializeAcademy()
//     {
//         foreach (CastleArea area in areas)
//         {
//             area.Initialize();
//         }
//     }
//
//     public override void AcademyReset()
//     {
//         if (areas == null)
//         {
//             areas = GameObject.FindObjectsOfType<CastleArea>();
//         }
//
//         foreach (CastleArea area in areas)
//         {
//             targetCount = (int)resetParameters["targetCount"];
//             area.targetCount = targetCount;
//             area.m_Team0AgentGroup.EndGroupEpisode();
//             area.m_Team1AgentGroup.EndGroupEpisode();
//             area.ResetArea();
//             if (m_ResetTimer % 300 == 0)
//             {
//                 area.m_Team0AgentGroup.EndGroupEpisode();
//                 area.m_Team1AgentGroup.EndGroupEpisode();
//             }
//         }
//
//         {
//             // if (!m_Initialized) return;
//             //RESET SCENE IF WE MaxEnvironmentSteps
//             m_ResetTimer += 1;
//
//             // if (m_ResetTimer % MaxEnvironmentSteps == 0)
//             // {
//             //     ResetArea();
//             // }
//
//         }
//     }
// }
//
