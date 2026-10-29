using UnityEngine;

internal static class LakeHit
{
    // A swept capsule covers the complete fixed-step path, including fast falling drops.
    public static bool Touch(Health target, Vector2 from, Vector2 to, float radius,
        ContactFilter2D filter, Collider2D[] contacts)
    {
        if (target == null || target.IsDead) return false;
        Vector2 d = to - from;
        int count = Physics2D.OverlapCapsule((from + to) * .5f,
            new Vector2(d.magnitude + 2 * radius, 2 * radius), CapsuleDirection2D.Horizontal,
            Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg, filter, contacts);
        for (int i = 0; i < count; i++)
            if (contacts[i] != null && contacts[i].GetComponentInParent<Health>() == target) return true;
        return false;
    }
    public static ContactFilter2D PlayerFilter()
    {
        var filter = new ContactFilter2D { useTriggers = true };
        filter.SetLayerMask(1 << LayerMask.NameToLayer("Player"));
        return filter;
    }
}
