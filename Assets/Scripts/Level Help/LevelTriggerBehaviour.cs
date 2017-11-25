using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Flags]
public enum PlayerType
{
    None = 0,
    Cat = 8,
    Rabbit = 64,
    CatAndRabbit = Cat | Rabbit
}

[RequireComponent(typeof(BoxCollider))]
public class LevelTriggerBehaviour : MonoBehaviour {

    [Serializable]
    public class LevelTrigger : UnityEvent<Collider> { public PlayerType neededCharacters; }

    [ReadOnly]
    [SerializeField]
    private PlayerType currentCharacters = PlayerType.None;
    public LevelTrigger OnEnter;
    public LevelTrigger OnStay;
    public LevelTrigger OnExit;

	private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController2 controller = other.GetComponent<PlayerController2>();

            switch (currentCharacters)
            {
                case PlayerType.None:
                    currentCharacters |= controller.characterType;
                    break;
                case PlayerType.Cat:
                case PlayerType.Rabbit:
                    if ((currentCharacters & controller.characterType) != controller.characterType)
                    {
                        currentCharacters |= controller.characterType;
                    }
                    break;
            }
            Trigger(OnEnter, other);
        }
        
    }
    private void OnTriggerStay(Collider other)
    {
        Trigger(OnStay, other);
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController2 controller = other.GetComponent<PlayerController2>();

            switch (currentCharacters)
            {
                case PlayerType.None: /* Do Nothing */ break;
                case PlayerType.Cat:
                case PlayerType.Rabbit:
                case PlayerType.CatAndRabbit:
                    if ((currentCharacters & controller.characterType) == controller.characterType)
                    {
                        currentCharacters &= ~controller.characterType;
                    }
                    break;
            }

            Trigger(OnExit, other);
        }

    }

    private void Trigger(LevelTrigger e, Collider other)
    {
        if (currentCharacters == e.neededCharacters)
        {
            e.Invoke(other);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(LevelTriggerBehaviour))]
public class LevelTriggerBehaviourEditor : Editor
{
    SerializedProperty
        sp_CurrentCharacters,
        sp_OnEnter,
        sp_OnStay,
        sp_OnExit;

    private void OnEnable()
    {
        sp_CurrentCharacters = serializedObject.FindProperty("currentCharacters");
        sp_OnEnter = serializedObject.FindProperty("OnEnter");
        sp_OnStay = serializedObject.FindProperty("OnStay");
        sp_OnExit = serializedObject.FindProperty("OnExit");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(sp_CurrentCharacters);

        OnDrawEvent(sp_OnEnter);
        OnDrawEvent(sp_OnStay);
        OnDrawEvent(sp_OnExit);

        serializedObject.ApplyModifiedProperties();
    }

    void OnDrawEvent(SerializedProperty eventProperty)
    {
        eventProperty.isExpanded = EditorGUILayout.Foldout(eventProperty.isExpanded, eventProperty.displayName);

        if (eventProperty.isExpanded)
        {
            EditorGUILayout.PropertyField(eventProperty.FindPropertyRelative("neededCharacters"));
            EditorGUILayout.PropertyField(eventProperty);
        }
    }
}
#endif
