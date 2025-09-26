// realvirtual (R) Framework for Automation Concept Design, Virtual Commissioning and 3D-HMI
// Copyright(c) 2019 realvirtual GmbH - Usage of this source code only allowed based on License conditions see https://realvirtual.io/en/company/license
using System.Collections.Generic;


namespace realvirtual
{
    public abstract class PathStrategy : realvirtualBehavior, ISelectNextPath
    {
        public abstract void SelectNextPath(PathMover pathMover, ref List<SimulationPath> Pathes);

        private List<PathMover> Blocked = new List<PathMover>();

        protected void AddBlocked(PathMover pathMover)
        {
            if (!Blocked.Contains(pathMover))
                Blocked.Add(pathMover);
        }

        protected new void Awake()
        {
            foreach (var transportable in Blocked.ToArray())
            {
                if (transportable.TryMoveNext())
                {
                    Blocked.Remove(transportable);
                    return;
                }
            }

            base.Awake();
        }

        protected void AwakeBlocked()
        {
            Invoke("Awake", 0.01f);
        }
    }
}
