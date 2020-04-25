using UnityEngine;


[System.Serializable]
public abstract class Weapon : ItemBase
{
    protected Name weaponName;
    protected WeaponType damageType;
    protected float damage;



    public abstract float Attack(Vector2 mousePosition, Vector2 originPosition);


    protected enum WeaponType
    {
        Direct,
        Splash
    }


    public enum Name
    {

    }

}
