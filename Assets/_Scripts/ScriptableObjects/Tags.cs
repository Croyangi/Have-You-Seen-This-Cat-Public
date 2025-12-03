using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Tags : MonoBehaviour
{
    [SerializeField] public Tag[] tags;

    /// <summary>
    /// Returns if a tag or tags are found.
    /// </summary>
    public bool SearchTag(Tag searchedTag)
    {
        return tags.Contains(searchedTag);
    }

    public bool SearchTag(Tag[] searchedTags, bool allMatching = false)
    {
        HashSet<Tag> tagSet = new HashSet<Tag>(tags);

        if (allMatching)
        {
            return searchedTags.All(tagSet.Contains);
        }
        else
        {
            return searchedTags.Any(tagSet.Contains);
        }
    }
}
