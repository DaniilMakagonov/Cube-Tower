using Unity.VisualScripting;
using UnityEngine;

public class ExplodeCube : MonoBehaviour
{
  public GameObject restartButton, explosion;
  private bool _collisionSet;
  private void OnCollisionEnter(Collision other)
  {
    if (other.gameObject.CompareTag("Cube") && !_collisionSet)
    {
      for (int i = other.transform.childCount - 1; i >= 0; --i)
      {
        Transform child = other.transform.GetChild(i);
        child.AddComponent<Rigidbody>();
        child.GetComponent<Rigidbody>().AddExplosionForce(70f, Vector3.up, 5f);
        child.SetParent(null);
      }
      restartButton.SetActive(true);
      if (Camera.main != null) Camera.main.transform.localPosition -= new Vector3(0, 0, 3);
      Camera.main.AddComponent<CameraShake>();

      GameObject vfx = Instantiate(explosion, new Vector3(other.contacts[0].point.x, other.contacts[0].point.y, other.contacts[0].point.z), Quaternion.identity);
      Destroy(vfx, 1.5f);
      if (PlayerPrefs.GetString("music") != "No")
        GetComponent<AudioSource>().Play();
      Destroy(other.gameObject);
      _collisionSet = true;
    }
    
  }
}
