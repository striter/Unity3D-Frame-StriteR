﻿using UnityEngine;
namespace BoundingCollisionTest
{
    public class GeometryTest_Sphere : MonoBehaviour
    {
        public GSphere m_Sphere;
        public GRay m_Ray;
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            bool intersect = UGeometry.RayBSIntersect(m_Sphere,m_Ray);
            Gizmos.color = intersect ? Color.green : Color.grey;
            Gizmos.DrawWireSphere(m_Sphere.center,m_Sphere.radius);
            Vector2 distances = UGeometry.RayBSDistance(m_Sphere, m_Ray);

            Gizmos.color = intersect ? Color.white:Color.grey;
            float rayDistance = 1f;
            if (distances.x >= 0)
            {
                Gizmos.color = Color.blue;
                rayDistance = distances.x;
                Gizmos.DrawSphere(m_Ray.GetPoint(distances.x), .1f);
            }
            if (distances.y >= 0)
            {
                Gizmos.color = Color.red;
                rayDistance = distances.y;
                Gizmos.DrawSphere(m_Ray.GetPoint(distances.y), .1f);
            }
            Gizmos.color = Color.white;
            Gizmos_Extend.DrawLine(m_Ray.ToLine(rayDistance));
        }
#endif
    }
}