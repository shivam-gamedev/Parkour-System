using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HIdeUI : MonoBehaviour
{
   [SerializeField]
        private GameObject[] m_objectsToHide = null;

    void Start()
    {
        
    }
 private bool m_uiHidden = false;
    // Update is called once per frame
    void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                m_uiHidden = !m_uiHidden;

                for (int i = 0; i < m_objectsToHide.Length; ++i)
                {
                    m_objectsToHide[i].SetActive(m_uiHidden);
                }
            }
        }
}
