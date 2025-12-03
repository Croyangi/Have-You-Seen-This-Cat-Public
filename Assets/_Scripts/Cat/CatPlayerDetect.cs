using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatPlayerDetect : MonoBehaviour
{
    [SerializeField] private Tag tag_player;
    [SerializeField] private CatStateHelper _stateHelper;

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.TryGetComponent<Tags>(out var _tags) && _tags.SearchTag(tag_player))
        {
            _stateHelper.SetWithPlayer(true);
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.TryGetComponent<Tags>(out var _tags) && _tags.SearchTag(tag_player))
        {
            _stateHelper.SetWithPlayer(false);
        }
    }
}
