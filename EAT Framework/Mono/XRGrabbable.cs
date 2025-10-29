using BNG;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace HenryLab
{
    public class XRGrabbable : XRBase, IGrabbableEntity
    {
        public new string Name => Str.Grabbable;

        /// <summary>
        /// ���ͷ�����ʱ�Ƿ���������ģ��
        /// ���Ϊtrue��������ܵ���������ײ������Ӱ��
        /// ���Ϊfalse���������ͷ�˲���ٶ�����Ϊ0��
        /// </summary>
        [Tooltip("����ʱ�ͷ�������ܹ���Ӱ��")]
        public bool usePhysicsOnRelease = false;

        /// <summary>
        /// ���崦��ץȡ״̬ʱ������ץȡ�������ģ�ⷽʽ
        /// </summary>
        [Tooltip("���崦��ץȡ״̬ʱ������ץȡ�������ģ�ⷽʽ")]
        public GrabPhysics GrabPhysics = GrabPhysics.Kinematic;

        /// <summary>
        /// ����ʱͬ����ץȡ���λ��
        /// </summary>
        [Tooltip("����ʱͬ����ץȡ���λ��")]
        public GrabType GrabMechanic = GrabType.Snap;

        public Transform destination = null;

        public Grabbable Grabbable
        {
            get
            {
                var g = GetComponent<Grabbable>();
                if(!g) g = gameObject.AddComponent<Grabbable>();
                g.GrabPhysics = GrabPhysics;
                g.GrabMechanic = GrabMechanic;
                return g;
            }
        }

        public Transform Destination => destination;

        public void OnGrabbed()
        {
            var obj = EntityManager.Instance.vrexplorerMono.gameObject;
            XRDirectInteractor interactor;
            if(!obj.TryGetComponent(out interactor))
            {
                interactor = obj.AddComponent<XRDirectInteractor>();
            }
            if(!obj.GetComponent<ActionBasedController>())
            {
                obj.AddComponent<ActionBasedController>();
            }
            var e = new SelectEnterEventArgs() { interactorObject = interactor };
            var h = new HoverEnterEventArgs() { interactorObject = interactor };
            var a = new ActivateEventArgs() { interactorObject = interactor };
            if(_interactable)
            {
                _interactable.selectEntered.Invoke(e);
                _interactable.hoverEntered.Invoke(h);
                _interactable.firstSelectEntered.Invoke(e);
                _interactable.firstHoverEntered.Invoke(h);
                _interactable.activated.Invoke(a);
            }
        }

        public void OnReleased()
        {
            if(!usePhysicsOnRelease && Grabbable.gameObject.GetComponent<Rigidbody>())
            {
                Grabbable.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            }
        }
    }
}