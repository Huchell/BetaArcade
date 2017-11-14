using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerManager : MonoBehaviour {

    // Singleton

    private PlayerController[] m_playerControllers;

    public int NumberOfPlayerControllers
    {
        get
        {
            return m_playerControllers.Length;
        }
    }

    public int NumberOfControlledPlayerControllers
    {
        get
        {
            return m_playerControllers.Where(pc => pc.playerNumber >= 0).ToArray().Length;
        }
    }

    private void Start()
    {
        m_playerControllers = FindObjectsOfType<PlayerController>();
    }
    private void FixedUpdate()
    {
        if (NumberOfControlledPlayerControllers == 1)
        {
            // 1 Player Stuff
            if (Input.GetKeyDown(KeyCode.P))
            {
                int index = FindCurrentPlayer();

                if (index < 0)
                    return;

                m_playerControllers[index].playerNumber = -1;

                int newIndex = (int)Mathf.Repeat(index + 1, NumberOfPlayerControllers);

                m_playerControllers[newIndex].playerNumber = 0;
            }

            if (Input.GetButtonDown("Jump_1"))
            {
                m_playerControllers[1].playerNumber = 1;

                m_playerControllers[0].camera.rect = new Rect(.0f, .0f, .5f, 1f);
                m_playerControllers[1].camera.rect = new Rect(.5f, .0f, .5f, 1f);
            }
        }
    }

    private int FindCurrentPlayer()
    {
        for (int i = 0; i  < NumberOfPlayerControllers; i++)
        {
            if (m_playerControllers[i].playerNumber >= 0)
            {
                return i;
            }
        }

        return -1;
    }

    public void AddPlayer(PlayerController controller)
    {
        if (!m_playerControllers.Contains(controller))
        {
            PlayerController[] newArray = new PlayerController[m_playerControllers.Length + 1];
            newArray.Intersect(m_playerControllers);
            newArray[newArray.Length - 1] = controller;

            m_playerControllers = newArray;
        }
    }
}
