using UnityEngine;
using System.Collections;

public class BoundaryMover : MonoBehaviour 
{
	public static Rect m_Boundary = new Rect( 0.0f - 7.5f , 0.0f - 8.5f , 15.0f , 17.0f ) ;
	public Vector2 m_PosXY = Vector2.zero ;
	
	// Use this for initialization
	void Start () 
	{
		m_PosXY.Set( this.transform.position.x 
		, this.transform.position.y ) ;
		if( false == m_Boundary.Contains( m_PosXY ) )
		{
			Vector3 targetPos = this.transform.position ;
			if( m_PosXY.x < m_Boundary.xMin )
			{
				targetPos.x = m_Boundary.xMin ;
			}
			else if( m_PosXY.x > m_Boundary.xMax )
			{
				targetPos.x = m_Boundary.xMax ;
			}
			
			if( m_PosXY.y < m_Boundary.yMin )
			{
				targetPos.y = m_Boundary.yMin ;
			}
			else if( m_PosXY.y > m_Boundary.yMax )
			{
				targetPos.y = m_Boundary.yMax ;
			}
			
			MoveToPos move = this.gameObject.AddComponent<MoveToPos>() ;
			move.m_DestinationPos = targetPos ;
		}
		
		Component.Destroy( this ) ;
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		
		
	}
}
