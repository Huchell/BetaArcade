using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoppingBoard : MonoBehaviour {
    
    [System.Serializable]
    public struct Knife
    {
        [Tooltip("The root transform of the knife")]
        public Transform knifeRoot;

        public AnimationCurve path;

        public float speed;
        public bool move;

        private float time;
        public float delay;

        private bool isCutting;
        private bool updateCutting;

        public IEnumerator ChopKnife()
        {
            // Set cutting to true
            isCutting = updateCutting = true;

            WaitForSeconds del = new WaitForSeconds(delay);

            yield return del;

            while (isCutting)
            {
                time += Time.deltaTime * speed;
                Vector3 curPos = knifeRoot.localPosition;
                curPos.y = path.Evaluate(time);
                knifeRoot.localPosition = curPos;

                yield return new WaitForFixedUpdate();

                if (time >= path.keys[path.length-1].time)
                {
                    time = 0;
                    yield return del;
                }
            }
        }
    }

    public Knife[] knives;
    public bool b_Chop = true;

    [SerializeField]
    private Knife defaultKnife;

    [SerializeField]
    private GameObject knifeObjectPrefab;

    public void ChopAllKnives()
    {
        for (int i = 0; i < knives.Length; i++)
        {
            if (knives[i].knifeRoot)
                StartCoroutine(knives[i].ChopKnife());
        }
    }

    public void ChopKnife(int index)
    {
        if (index > 0 && index < knives.Length)
        {
            StartCoroutine(knives[index].ChopKnife());
        }
    }
}
