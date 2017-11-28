using GameFW.Utility;
using UnityEngine;

namespace MobaServer.Logic.Fight
{
    public enum MoveStatus {
        Moving,
        Still,
    }

    public class EntityMoveInfo
    {
        public CVector3 cVector3;
        public MoveStatus moveStatus = MoveStatus.Still;

        public EntityMoveInfo(CVector3 cVector3, bool isMoving) {
            this.cVector3 = cVector3;
            this.moveStatus = isMoving ? MoveStatus.Moving : MoveStatus.Still;
        }

        public EntityMoveInfo(Vector3 vector3, bool isMoving) {
            this.cVector3 = new CVector3(vector3);
            this.moveStatus = isMoving ? MoveStatus.Moving : MoveStatus.Still;
        }

    }
}
