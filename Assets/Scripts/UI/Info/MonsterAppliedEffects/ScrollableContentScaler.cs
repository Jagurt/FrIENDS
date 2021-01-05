 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollableContentScaler : MonoBehaviour
{
    [SerializeField] ContentSizeFitter baseContentSizeFitter;
    RectTransform contentParentRect;
    ContentSizeFitter contentSizeFitter;
    Transform content;

    float prefferedHeight = 10;
    [SerializeField] float maximumHeight = 170;

    void Start()
    {
        contentParentRect = baseContentSizeFitter.transform.Find("ContentParentRect").GetComponent<RectTransform>();
        contentSizeFitter = contentParentRect.GetComponent<ContentSizeFitter>();
        content = contentParentRect.Find("Content");

        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        InvokeRepeating("RescaleContainer", 0.05f, 0.5f);
    }
    private void OnDisable()
    {
        CancelInvoke();
    }

    void RescaleContainer()
    {
        prefferedHeight = 10;

        for (int i = 0; i < content.childCount; i++)
        {
            LayoutElement childLE = content.GetChild(i).GetComponent<LayoutElement>();
            prefferedHeight += childLE.preferredHeight;

            //Debug.Log("Checking child - " + i + ", prefferedHeight - " + prefferedHeight);
        }

        if (contentSizeFitter.enabled && prefferedHeight >= maximumHeight)
        {
            contentSizeFitter.enabled = false;
            baseContentSizeFitter.enabled = false;
            contentParentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, maximumHeight);
        }
        else if (!contentSizeFitter.enabled && prefferedHeight < maximumHeight)
        {
            contentSizeFitter.enabled = true;
            baseContentSizeFitter.enabled = true;
        }
    }

    internal Transform GetContentTransform()
    {
        return content;
    }
}
