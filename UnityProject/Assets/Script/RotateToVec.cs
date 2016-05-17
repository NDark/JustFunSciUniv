using UnityEngine;
using System.Collections;

public class RotateToVec : MonoBehaviour 
{
	public bool m_ActiveRotate = false ;
	public Transform m_ReferenceTransform ;
	public float m_RotateSpeed = 0.1f ;
	public float m_DotValue = 0.0f ;
	
	public void CalculateTargetPose()
	{
		m_TargetPose = MagnetManager.CalculateMagnetDirection( 
		                                                      this.m_ReferenceTransform 
		                                                      , this.gameObject.transform.position ) ;
		
		m_DotValue = Quaternion.Angle( m_TargetPose , this.transform.rotation ) ;
		
		if( m_DotValue > 1.0f )
		{
			m_ActiveRotate = true ;		
		}
		else
		{
			m_ActiveRotate = false ;		
		}
		
	}
	// Use this for initialization
	void Start () 
	{
	
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		if( true == m_ActiveRotate )
		{
			CalculateTargetPose() ;

			this.transform.rotation = Quaternion.Lerp( this.transform.rotation 
			                                          , m_TargetPose 
			                , m_RotateSpeed ) ;			

			
		}
		
	}
	
	private Quaternion m_TargetPose = Quaternion.identity ;
}
