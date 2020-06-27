using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPPZ.Geometry.Model;

public interface ISubDivScheme <Subject> where Subject : Subdividable
{
    List<Subject> GetChildren(Subject parent);
}