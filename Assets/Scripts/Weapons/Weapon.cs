using UnityEngine;


[System.Serializable]
[CreateAssetMenu]
public abstract class Weapon : ScriptableObject
{
    protected WeaponName weaponName;
    protected WeaponType damageType;
    protected float damage;



    public abstract float Attack(Vector2 mousePosition, Vector2 originPosition);


    protected enum WeaponType
    {
        Direct,
        Splash
    }


    public enum WeaponName
    {

    }

}
