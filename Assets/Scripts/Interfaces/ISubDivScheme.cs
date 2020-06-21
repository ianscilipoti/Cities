using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPPZ.Geometry.Model;

public interface ISubDivScheme
{
    List<ISubdividable> GetChildren(ISubdividable parent, out List<Vector4> edges);
}
