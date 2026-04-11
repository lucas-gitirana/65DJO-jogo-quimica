using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
public class BotaoPressionado : MonoBehaviour
{
 public int numBotao;
 void Start()
 {
 GetComponent<XRSimpleInteractable>().selectEntered.AddListener(x=>Pressionei());
 }
 public void Pressionei()
 {
 GamePuzzleEvents.RaiseLabButtonPressed(numBotao);
 }
}