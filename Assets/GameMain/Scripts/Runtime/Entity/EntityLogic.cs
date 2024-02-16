using UnityEngine;
using UnityGameFramework.Runtime;

namespace GameMain.Runtime
{
    public abstract class EntityLogic<TEntityData> : UnityGameFramework.Runtime.EntityLogic
        where TEntityData : EntityData
    {
        public int Id => Entity.Id;
        public TEntityData EntityData { get; set; }

        protected override void OnShow(object userData)
        {
            base.OnShow(userData);
            EntityData = userData as TEntityData;
            if (EntityData == null)
            {
                Log.Error("EntityData type is Error!!!");
                return;
            }
            Name = GameFramework.Utility.Text.Format("[Entity {0}]", Id);
            CachedTransform.position = EntityData.Position;
            CachedTransform.rotation = EntityData.Rotation;
            CachedTransform.localScale = Vector3.one;
        }

        public virtual void HideSelf()
        {
            GameModule.Entity.HideEntity(Id);
        }
    }
}