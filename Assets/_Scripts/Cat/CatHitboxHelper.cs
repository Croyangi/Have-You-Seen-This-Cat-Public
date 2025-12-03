using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatHitboxHelper : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform catTransform;
    [SerializeField] private Tag tag_player;

    public bool IsPlayerInRadius(float radius, Transform transform)
    {
        List<Collider> colliders = new List<Collider>(Physics.OverlapSphere(transform.position, radius));

        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent<Tags>(out var _tags) && _tags.SearchTag(tag_player))
            {
                return true;
            }
        }
        return false;
    }
}
