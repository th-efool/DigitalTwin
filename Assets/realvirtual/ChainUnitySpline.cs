// realvirtual.io (formerly game4automation) (R) a Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/unternehmen/lizenz  

using System;
using NaughtyAttributes;
using UnityEngine;
#if REALVIRTUAL_SPLINES
using UnityEngine.Splines;
#endif
namespace realvirtual
{
#if REALVIRTUAL_SPLINES
  public class ChainUnitySpline : SplineComponent,IChain
  {

      [Tooltip("Reference to the Unity Spline Container component")]
      public SplineContainer splineContainer;
      private float lastclosestperc;

      private void Awake()
      {
          splineContainer = GetComponent<SplineContainer>();
          if(splineContainer==null)
              Debug.LogError("No SplineContainer found. Please add a SplineContainer to the GameObject");
      }

      public Vector3 GetClosestDirection(Vector3 position)
      {
         
              lastclosestperc= ClosestPoint(position,100);
              return splineContainer.EvaluateTangent(lastclosestperc);
      }

      public Vector3 GetClosestPoint(Vector3 position)
      {
          lastclosestperc= ClosestPoint(position,100);
          return splineContainer.EvaluatePosition(lastclosestperc);
      }

      public Vector3 GetPosition(float normalizedposition, bool normalized = true)
      {
          return splineContainer.EvaluatePosition(normalizedposition);
      }

      public Vector3 GetDirection(float normalizedposition, bool normalized = true)
      {
         // if (normalized) normalizedposition = Reparam(normalizedposition);
          return splineContainer.EvaluateTangent(normalizedposition);
      }
      public Vector3 GetUpDirection(float normalizedposition, bool normalized = true)
      {
          Vector3 dir=Vector3.zero;
          dir=splineContainer.EvaluateUpVector(normalizedposition);
          return dir;
      }

      public float CalculateLength()
      {
          if(splineContainer==null)
              splineContainer = GetComponent<SplineContainer>();

          return splineContainer.CalculateLength();
      }
      

      public bool UseSimulationPath()
      {
          return false;
      }
      public float ClosestPoint(Vector3 point, int divisions = 100)
      {
          //make sure we have at least one division:
          if (divisions <= 0) divisions = 1;

          //variables:
          float shortestDistance = float.MaxValue;
          Vector3 position = Vector3.zero;
          Vector3 offset = Vector3.zero;
          float closestPercentage = 0;
          float percentage = 0;
          float distance = 0;

          //iterate spline and find the closest point on the spline to the provided point:
          for (float i = 0; i < divisions + 1; i++)
          {
              percentage = i / divisions;
              position = GetPosition(percentage);
              offset = position - point;
              distance = offset.sqrMagnitude;

              //if this point is closer than any others so far:
              if (distance < shortestDistance)
              {
                  shortestDistance = distance;
                  closestPercentage = percentage;
              }
          }

          return closestPercentage;
      }
      
  }
  #else
  public class ChainUnitySpline : MonoBehaviour
    {
     [Tooltip("Reference to the Chain component (requires Unity Spline package)")]
     [InfoBox("This Demo Chain only works with Unity Spline. Please install the Unity Spline package or update your scripting define symbols in ProjectSettings/Player.")]
      public Chain Chain;

        void Awake()
        {
            Chain = GetComponent<Chain>();
            if (Chain != null)
            {
                Chain.gameObject.SetActive(false);
                Debug.Log("Unity Spline is not supported in this version. Examples with Unity Spline will be deactivated.");
            }
        }
    }
#endif
}

