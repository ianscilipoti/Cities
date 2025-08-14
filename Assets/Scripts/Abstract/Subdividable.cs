using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EPPZ.Geometry.Model;


//provides basic structural functionality to recursively subdivide an object
public interface Subdividable
{
    bool Subdivide();

    bool IsSubdividable();

    bool IsSubdivided();

}
