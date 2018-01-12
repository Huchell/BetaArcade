using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnifeBoss : MonoBehaviour
{
    GameObject rabbit;
    GameObject cat;

    //Knife to player offset
    Vector3 rabbitPosOffset;
    public Vector3 posOffest;

    enum KnifeStates { Moving, Attack, Stuck, Recover };
    KnifeStates state;


    Vector2 time;
    bool isRandomed = false;
    float randTime;
    float modTimeDivide;
    float OurTime = 0;

    //---------------Attack---------------

    public int AttackPrepTimer = 0;
    public int KnifeRecoverDelayTimer = 0;
    bool DoOnceAttackPrep = false;
    bool doOnceRecoverMiss = false;
    bool HasAttacked = false;
    bool HasRecovered = false;

    //---------------Stuck---------------

    public bool isStuck = false;
    public bool isHit = false;

    int stuckCount = 0;
    int stuckIfCount = 0;

    //-------------Health---------------

    public int rabbitHealth = 3;
    public int knifeHealth = 3;

    // Use this for initialization
    void Start()
    {
        rabbit = FindObjectOfType<RabbitController>().gameObject;
        cat = FindObjectOfType<CatController>().gameObject;
        time = new Vector2(knifeHealth, knifeHealth * 2);
        modTimeDivide = knifeHealth * 2 + 1;
        state = KnifeStates.Moving;
    }



    // Update is called once per frame
    void Update()
    {
        rabbitPosOffset = rabbit.transform.position + posOffest;

        if (state == KnifeStates.Moving)
        {
            ResetVariables();

            KnifeMoving();

            if (OurTime % modTimeDivide >= randTime)
            {
                //Debug.Log("Attacking");
                state = KnifeStates.Attack;
                OurTime = 0;
            }
        }

        if (state == KnifeStates.Attack)
        {
            isRandomed = false;
            if (!DoOnceAttackPrep)
            {
                KnifeAttackPrep();
                DoOnceAttackPrep = true;
            }
            if (!HasAttacked)
            {
                AttackPrepTimer++;
            }
            if (AttackPrepTimer >= 25)
            {
                HasAttacked = true;
                KnifeAttack();
                AttackPrepTimer = 0;
            }
            if (!HasRecovered && HasAttacked)
            {
                KnifeRecoverDelayTimer++;
            }
            if (KnifeRecoverDelayTimer >= 200)
            {
                HasRecovered = true;
                KnifeRecoverDelayTimer = 0;
            }
            if (HasAttacked && HasRecovered)
            {
                state = KnifeStates.Recover;
            }
        }
        if (state == KnifeStates.Recover)
        {
            if (!doOnceRecoverMiss)
            {
                KnifeRecoverMiss();
                doOnceRecoverMiss = true;
            }
        }
        if (state == KnifeStates.Stuck)
        {
            KnifeStuck();
        }
    }

    void KnifeMoving()
    {
        transform.position = Vector3.MoveTowards(transform.position, rabbitPosOffset, 5 * Time.deltaTime);

        if (!isRandomed)
        {
            randTime = Random.Range(time.x, time.y);
            //Debug.Log(randTime);
            isRandomed = true;
        }
    }

    private void KnifeAttackPrep()
    {
        StartCoroutine(c_KnifeAttackPrep());
    }

    IEnumerator c_KnifeAttackPrep()
    {
        Vector3 endPos = transform.position + new Vector3(0, 2, 1);
        while (!HasAttacked)
        {
            transform.position = Vector3.Lerp(transform.position, endPos, 5 * Time.deltaTime);
            yield return null;
        }
    }

    private void KnifeAttack()
    {
        StartCoroutine(c_KnifeAttack());
    }

    IEnumerator c_KnifeAttack()
    {
        Vector3 endPos = transform.position + new Vector3(0, -4, -1);
        while (!HasRecovered && !isStuck)
        {
            transform.position = Vector3.Lerp(transform.position, endPos, 8 * Time.deltaTime);
            yield return null;
        }
    }

    private void KnifeRecoverMiss()
    {
        StartCoroutine(c_KnifeRecoverMiss());
    }

    IEnumerator c_KnifeRecoverMiss()
    {
        Vector3 endPos = rabbitPosOffset;
        //Debug.Log("Lerp Back");
        while (transform.position != endPos)
        {
            transform.position = Vector3.MoveTowards(transform.position, endPos, 20 * Time.deltaTime);
            yield return null;
        }
        state = KnifeStates.Moving;
    }

    IEnumerator c_KnifeRecoverStuck()
    {
        Vector3 endPos = rabbitPosOffset;
        while (transform.position != endPos)
        {
            transform.position = Vector3.MoveTowards(transform.position, endPos, 20 * Time.deltaTime);
            isStuck = false;
            yield return null;
        }


    }



    private void KnifeStuck()
    {
        if (isStuck && stuckIfCount < 400)
        {
            stuckCount++;

            if (stuckCount >= 400)
            {
                //StartCoroutine(c_KnifeRecoverStuck());
                KnifeRecoverMiss();
                isStuck = false;
            }
            else if (cat.GetComponent<CatController>().isCharging && isHit)
            {
                //Debug.Log("Cat Hit Knife");
                knifeHealth -= 1;
                stuckCount = 400;
                if (knifeHealth > 0)
                {
                    KnifeRecoverMiss();
                }
                else
                {
                    DestroyObject(gameObject);
                    //Debug.Log("Player Won Game");
                }
            }
            
            stuckIfCount++;
        }
    }

    private void ResetVariables()
    {
        OurTime = OurTime + Time.deltaTime;
        DoOnceAttackPrep = false;
        doOnceRecoverMiss = false;
        HasAttacked = false;
        HasRecovered = false;
        isHit = false;
        stuckIfCount = 0;
        stuckCount = 0;
        //transform.position = rabbitPosOffset;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (state == KnifeStates.Attack && collision.GetComponent<RabbitController>()) // TEMP COMMENT-----------------------------------------------------------------
        {
            //Debug.Log("Rabbit Hit collider");
            //rabbit.GetComponent<RabbitController>().RabbitHealth -= 1;
            //rabbitHealth -= 1;
            collision.GetComponent<RabbitController>().TakeDamage(1);
        }
        if (state == KnifeStates.Attack && collision.name == "Cheese") // TEMP COMMENT-----------------------------------------------------------------
        {
            isStuck = true;
            state = KnifeStates.Stuck;
        }
        //else
        //{
        //    isStuck = false;
        //}

        if (isStuck && collision.name == "CatKnifeHitCol")
        {
            isHit = true;
        }
    }
}