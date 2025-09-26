// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license

using UnityEngine;

namespace realvirtual
{
	public class realvirtualUI : BehaviorInterface {
		
	
	
		public void SetColor(GameObject obj, Color color)
		{
			var renderers =  obj.GetComponentsInChildren<MeshRenderer>();
			foreach (Renderer render in renderers)
			{
				MaterialPropertyBlock props = new MaterialPropertyBlock();
				props.SetColor("_Color",color);
				props.SetColor("_Emission",color);
				render.SetPropertyBlock(props);
			}
		}
	
		public void ResetColor(GameObject obj)
		{
			var renderers =  obj.GetComponentsInChildren<MeshRenderer>();
			foreach (Renderer render in renderers)
			{
				render.SetPropertyBlock(null);
			}
		}
		
	}
}
