using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UnityProgrammerTask.Core
{
   [Flags]
   public enum TransformSaveFlags
   {
      None = 0,
      Parent = 1 << 0,
      PositionX = 1 << 1,
      PositionY = 1 << 2,
      PositionZ = 1 << 3,

      Position = PositionX | PositionY | PositionZ,

      RotationX = 1 << 4,
      RotationY = 1 << 5,
      RotationZ = 1 << 6,

      Rotation = RotationX | RotationY | RotationZ,
   }

   [Serializable]
   public class SaveableTransformHandler : ISaveableComponentHandler
   {
      [SerializeField]
      private TransformSaveFlags m_SaveFlags;

      [SerializeField]
      private Space m_PositionSpace = Space.World;

      [SerializeField]
      private Space m_RotationSpace = Space.Self;

      public void Load(GameObject gameObject, BinaryReader stream)
      {
         if ((m_SaveFlags & TransformSaveFlags.Parent) != 0)
         {
            GUID parentId = GUID.Read(stream);

            if (parentId != GUID.Empty)
            {
               SaveableGameObject parent = SaveableGameObject.GetSaveableGameObject(parentId);
               gameObject.transform.SetParent(parent.transform, true);
            }
         }

         Vector3 position = m_PositionSpace == Space.World
            ? gameObject.transform.position
            : gameObject.transform.localPosition;

         Vector3 rotation = m_RotationSpace == Space.World
            ? gameObject.transform.eulerAngles
            : gameObject.transform.localEulerAngles;

         if ((m_SaveFlags & TransformSaveFlags.PositionX) != 0)
            position.x = stream.ReadSingle();
         if ((m_SaveFlags & TransformSaveFlags.PositionY) != 0)
            position.y = stream.ReadSingle();
         if ((m_SaveFlags & TransformSaveFlags.PositionZ) != 0)
            position.z = stream.ReadSingle();

         if ((m_SaveFlags & TransformSaveFlags.RotationX) != 0)
            rotation.x = stream.ReadSingle();
         if ((m_SaveFlags & TransformSaveFlags.RotationY) != 0)
            rotation.y = stream.ReadSingle();
         if ((m_SaveFlags & TransformSaveFlags.RotationZ) != 0)
            rotation.z = stream.ReadSingle();

         if (m_PositionSpace == Space.World)
            gameObject.transform.position = position;
         else
            gameObject.transform.localPosition = position;

         if (m_RotationSpace == Space.World)
            gameObject.transform.eulerAngles = rotation;
         else
            gameObject.transform.localEulerAngles = rotation;

      }

      public void Save(GameObject gameObject, BinaryWriter stream)
      {
         if ((m_SaveFlags & TransformSaveFlags.Parent) != 0)
         {
            GUID parentId = GUID.Empty;
            Transform parent = gameObject.transform.parent;

            if (parent != null && parent.TryGetComponent(out SaveableGameObject saveable))
               parentId = saveable.Id;

            parentId.Write(stream);
         }

         Vector3 position = m_PositionSpace == Space.World
            ? gameObject.transform.position
            : gameObject.transform.localPosition;

         Vector3 rotation = m_RotationSpace == Space.World
            ? gameObject.transform.eulerAngles
            : gameObject.transform.localEulerAngles;

         if ((m_SaveFlags & TransformSaveFlags.PositionX) != 0)
            stream.Write(position.x);
         if ((m_SaveFlags & TransformSaveFlags.PositionY) != 0)
            stream.Write(position.y);
         if ((m_SaveFlags & TransformSaveFlags.PositionZ) != 0)
            stream.Write(position.z);

         if ((m_SaveFlags & TransformSaveFlags.RotationX) != 0)
            stream.Write(rotation.x);
         if ((m_SaveFlags & TransformSaveFlags.RotationY) != 0)
            stream.Write(rotation.y);
         if ((m_SaveFlags & TransformSaveFlags.RotationZ) != 0)
            stream.Write(rotation.z);
      }

      public void GetDependencies(GameObject gameObject, List<SaveableGameObject> dependencies)
      {
         if ((m_SaveFlags & TransformSaveFlags.Parent) != 0)
         {
            Transform parent = gameObject.transform.parent;
            if (parent != null && parent.TryGetComponent(out SaveableGameObject saveable))
               dependencies.Add(saveable);
         }
      }
   }
}