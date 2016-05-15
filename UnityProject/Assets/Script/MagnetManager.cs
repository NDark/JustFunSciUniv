using UnityEngine;
using System.Collections.Generic;

public class MagnetManager : MonoBehaviour 
{
	public GameObject m_MagnetPrefab = null ;
	public List<GameObject> m_Magnets = new List<GameObject>() ;
	public GameObject m_VirtualMagnet = null ;
	public GameObject m_TargetMagnets = null ;
	
	bool m_IsModied = false ;

	// Use this for initialization
	void Start () 
	{
		if( null != m_MagnetPrefab )
		{
			GameObject addObj = (GameObject) GameObject.Instantiate( m_MagnetPrefab ) ;
			if( null != addObj )
			{
				addObj.name = "magnet" ;
				m_VirtualMagnet = addObj ;
			}
		}

	}
	
	// Update is called once per frame
	void Update () 
	{
		if( false == m_IsModied )
		{
			UpdateMagnet() ;
		}
	}
	
	void UpdateMagnet()
	{
		if( null != m_VirtualMagnet )
		{
			RotateToVec rotate = m_TargetMagnets.GetComponent<RotateToVec>() ;
			if( null != rotate )
			{
				rotate.m_ActiveRotate = true ;
				rotate.m_TargetTransform = m_VirtualMagnet.transform ;
			}
		}
	}
	
}
