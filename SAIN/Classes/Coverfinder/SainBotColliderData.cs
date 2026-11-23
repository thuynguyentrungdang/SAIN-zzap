using UnityEngine;

namespace SAIN.SAINComponent.SubComponents.CoverFinder;

public struct SainBotColliderData(Collider collider)
{
    public Collider Collider = collider;
    public CoverPoint CoverPoint = null;
    public float SqrMagnitude;

    // perf: negative cache. A collider that gave no cover point is not re-analysed (path + raycasts) until the
    // timer runs out or the enemy / bot has moved enough to change the answer.
    public float NoCoverUntil;
    public Vector3 FailedTargetPos;
    public Vector3 FailedBotPos;

    public const float NegativeCacheSeconds = 3f;
    public const float NegativeCacheMoveSqr = 16f; // 4 m

    public readonly bool IsNegativeCached(float time, Vector3 targetPos, Vector3 botPos)
    {
        return time < NoCoverUntil
            && (targetPos - FailedTargetPos).sqrMagnitude < NegativeCacheMoveSqr
            && (botPos - FailedBotPos).sqrMagnitude < NegativeCacheMoveSqr;
    }

    public static bool operator ==(SainBotColliderData left, SainBotColliderData right)
    {
        return Equals(left.Collider, right.Collider);
    }

    public static bool operator !=(SainBotColliderData left, SainBotColliderData right)
    {
        return !(left == right);
    }

    public static bool operator ==(SainBotColliderData left, Collider right)
    {
        return Equals(left.Collider, right);
    }

    public static bool operator !=(SainBotColliderData left, Collider right)
    {
        return !(left == right);
    }

    public static bool operator ==(Collider left, SainBotColliderData right)
    {
        return Equals(left, right.Collider);
    }

    public static bool operator !=(Collider left, SainBotColliderData right)
    {
        return !(left == right);
    }

    public override bool Equals(object obj)
    {
        if (obj is SainBotColliderData other)
        {
            return this == other;
        }
        if (obj is Collider collider)
        {
            return this == collider;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return Collider != null ? Collider.GetHashCode() : 0;
    }
}
