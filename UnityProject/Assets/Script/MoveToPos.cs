using UnityEngine;
using System.Collections;

public class MoveToPos : MonoBehaviour 
{
	public Vector3 m_DestinationPos = Vector3.zero ;
	public float m_Threashold = 0.001f ;
	public float m_Speed = 5.0f ;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		if( Vector3.Distance( this.transform.position 
			, m_DestinationPos ) > m_Threashold )
		{
			this.transform.position = 
				Vector3.Lerp( this.transform.position 
					, m_DestinationPos 
					, m_Speed * Time.deltaTime ) ;
			
		}	
		else
		{
		
			Component.Destroy( this ) ;
		}
	}
}
