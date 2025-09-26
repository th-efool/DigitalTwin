// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

namespace realvirtual
{
  using System;
  using System.Collections.Generic;
  using UnityEngine;
  using UnityEngine.EventSystems;
  using UnityEngine.UI;

  public class rvUIWindowSortOrder : MonoBehaviour, IPointerClickHandler
  {
    private Canvas layoutElement;
    private List<Canvas> layoutElements = new List<Canvas>();

    private void Awake()
    {
      layoutElement = GetComponentInParent<Canvas>();
      layoutElements.AddRange(FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None));
    }

    private void OnEnable()
    {
      if (layoutElement != null)
      {
        layoutElement.sortingOrder = 3;
      }

      // set all other canvases to 1
      foreach (var element in layoutElements)
      {
        if (element != layoutElement)
        {
          element.sortingOrder = 2;
        }
      }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
      if (layoutElement != null)
      {
        layoutElement.sortingOrder = 3;
      }

      // set all other canvases to 1
      foreach (var element in layoutElements)
      {
        if (element != layoutElement)
        {
          element.sortingOrder = 2;
        }
      }
    }
  }
}