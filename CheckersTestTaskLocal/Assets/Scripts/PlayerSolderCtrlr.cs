
using UnityEngine;

public class PlayerSolderCtrlr : MonoBehaviour
{
    public void pickASolder()
    {
        SceneManager.Instance.playerSolderPickMngr(gameObject);
    }
}
