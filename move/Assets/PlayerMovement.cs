using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public GameObject ob_player;
    public GameObject ob_Wall1;
    List<Vector2> result;
    private void Start()
    {
        result = new List<Vector2>();
        Astar.Instance.AddWall(ob_Wall1);
    }
    bool moving = false;
    float TimePlus = 0;
    void Update()
    {

        TimePlus += Time.deltaTime;
        if(TimePlus > 1f)
        {
            TimePlus = 0f;
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (moving == false)
            {
            Vector2 MousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            MousePos.x = Mathf.Round(MousePos.x);
            MousePos.y = Mathf.Round(MousePos.y);

            Astar.Instance.Find(ob_player.transform.position, MousePos);
            }
        }

        if (Astar.Instance.UpdateFlagWithStateInit())
            {
            print("Success");
                Astar.Instance.GetRoute(ref result);
                StartCoroutine("MoveCoroutine");
            }
    }


    IEnumerator MoveCoroutine()
    {
        moving = true;
        foreach (var i in result)
        {
            ob_player.transform.position = new Vector3(i.x, i.y, ob_player.transform.position.z);
            yield return new WaitForSeconds(0.1f);
        }
        moving = false;
        result.Clear();
    }

}