
using UnityEngine;

public class BoardUnitCtrlr : MonoBehaviour
{
    public void makeAStepOnBoardUnit()
    {
        SceneManager.Instance.makeAStep(transform.localPosition);
    }
}
