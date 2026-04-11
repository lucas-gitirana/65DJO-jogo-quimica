using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
public class EventosPortaCorrer : MonoBehaviour
{
 private bool isOpen = false;
 private ConfigurableJoint joint;
 public TeleportationArea teleporte;
 void Start()
 {
 joint = GetComponent<ConfigurableJoint>();
 }
 float GetJointLinearX()
 {
 // Calcula posição do anchor no mundo
 Vector3 worldAnchor = joint.transform.TransformPoint(joint.anchor);
 Vector3 connectedAnchor = joint.connectedAnchor;
 // Delta entre anchors
 Vector3 delta = worldAnchor - connectedAnchor;
 // Eixo X do joint no espaço global
 Vector3 axisX = joint.transform.TransformDirection(Vector3.right);
 // Projeção do deslocamento no eixo X
 float displacementX = Vector3.Dot(delta, axisX);
 return displacementX;
 }
 void Update()
 {
 float abertura = Mathf.Abs(GetJointLinearX());
 // abriu
 if (!isOpen && abertura >= 0.6)
 {
 isOpen = true;
 teleporte.enabled = true;
 }
 else
 {
 // Porta fechou
 if (isOpen && abertura < 0.6)
 {
 isOpen = false;
 teleporte.enabled = false;
 }
 }
 }
}