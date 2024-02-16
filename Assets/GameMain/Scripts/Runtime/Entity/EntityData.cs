using GameFramework;
using UnityEngine;

namespace GameMain.Runtime
{
    [System.Serializable]
    public class EntityData : IReference
    {
        [SerializeField] private int m_Id = 0;
        [SerializeField] private int m_TypeId = 0;//表格配置Id
        [SerializeField] private Vector3 m_Position = Vector3.zero;
        [SerializeField] private Quaternion m_Rotation = Quaternion.identity;

        public int Id => m_Id;
        public int TypeId => m_TypeId;

        public Vector3 Position
        {
            get => m_Position;
            set => m_Position = value;
        }

        public Quaternion Rotation
        {
            get => m_Rotation;
            set => m_Rotation = value;
        }

        public EntityData()
        {
            m_Id = 0;
            m_TypeId = 0;
            m_Position = Vector3.zero;
            m_Rotation = Quaternion.identity;
        }

        protected void Fill(int id, int typeId)
        {
            m_Id = id;
            m_TypeId = typeId;
        }
        
        public virtual void Clear()
        {
            m_Id = 0;
            m_TypeId = 0;
            m_Position = Vector3.zero;
            m_Rotation = Quaternion.identity;
        }
    }
}
