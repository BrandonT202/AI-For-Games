using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyScript : MonoBehaviour {

    public bool canMove;
    public bool isMoving;
    public GameObject Target;
    public float fieldOfView;
    public float Range;
    public bool CanSee;
    public bool isNear;
    public bool isChasing;
    public List<Vector3> IdlePathNodes;
    public int currentIdleNode = 0;
    public float angle;


    // Use this for initialization
    void Start() {
        IdlePathNodes[0] = transform.position;
    }	

    void Reset()
    {
        if (GameObject.Find("Player"))
            Target = GameObject.Find("Player");
        if (GameObject.Find("Agent"))
            Target = GameObject.Find("Agent");
        if (IdlePathNodes.Count < 1)
            IdlePathNodes.Add(transform.position);
        else
            IdlePathNodes[0] = transform.position;
    }





    // Update is called once per frame
    void Update() {
        if (Vector3.Distance(Target.transform.position, transform.position) <= Range)
            isNear = true;
        else
            isNear = false;
        //double angle = DoAngleMath(transform.position, Target.transform.position);
        Vector3 targetDir = Target.transform.position - transform.position;
        angle = Vector3.Angle(targetDir, transform.forward) - transform.rotation.y * 2;
        if (angle < transform.rotation.y + fieldOfView && angle > transform.rotation.y - fieldOfView) {
            
                CanSee = true;
        }
        else
            CanSee = false;


        if (CanSee && isNear)
            isChasing = true;
        else
            isChasing = false;

        if (!isChasing)
            Idle();
    }


    void Idle() {
        
    }


    double DoAngleMath(Vector3 pos, Vector3 target)
    {
        float dx;
        float dy;
        double theta;

        dx = target.x - pos.x;
        dy = target.z - pos.z;
        dy = -dy;  // convert from screen-space
                   // to proper mathematical space.

        if (dx == 0)
        {
            theta = 0.0;
        }
        else
        {

            /*  if not using atan2
                theta = Math.atan(dy/dx);
            */


            theta = Mathf.Atan2(dy, dx);
            theta = theta * 360 * 0.5 / Mathf.PI;


            /*  if not using atan2
                if (dx < 0)
                  {
                  if (dy > 0)
                    theta += 180.0;
                  else
                    theta -= 180.0;
                  }
            */
        }
        return theta;
    }
    
}
