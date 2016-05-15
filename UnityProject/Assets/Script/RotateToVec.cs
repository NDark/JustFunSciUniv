using UnityEngine;
using System.Collections;

public class RotateToVec : MonoBehaviour 
{
	public bool m_ActiveRotate = false ;
	public Transform m_ReferenceTransform ;
	public float m_RotateSpeed = 0.1f ;
	public float m_DotValue = 0.0f ;
	
	// Use this for initialization
	void Start () 
	{
	
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		if( true == m_ActiveRotate )
		{
			m_DotValue = Vector3.Dot ( this.gameObject.transform.forward 
			                          , m_ReferenceTransform.forward ) ;
			
			this.transform.rotation = MagnetManager.CalculateMagnetDirection( this.m_ReferenceTransform 
				, this.gameObject.transform.position ) ;
			
			if( m_DotValue < 1.0f )
			{
				
				/*
					Quaternion.Lerp( this.transform.rotation 
					                , m_TargetTransform.rotation 
					                , m_RotateSpeed ) ;			
					                */
			}
			else
			{
				m_ActiveRotate = false ;
			}
		}
		
	}
}
