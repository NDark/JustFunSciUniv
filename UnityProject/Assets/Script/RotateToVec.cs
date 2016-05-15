using UnityEngine;
using System.Collections;

public class RotateToVec : MonoBehaviour 
{
	public GameObject targetObj = null ;
	public float dotValue = 0.0f ;
	// Use this for initialization
	void Start () 
	{
	
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		if( targetObj )
		{
			dotValue = Vector3.Dot ( targetObj.transform.forward , this.gameObject.transform.forward ) ;
			if( dotValue < 1.0f )
			{
				targetObj.transform.rotation = 
					Quaternion.Lerp( targetObj.transform.rotation , this.gameObject.transform.rotation , 0.1f ) ;			
			}
		}
	
	}
}
