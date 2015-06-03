#include "StdAfx.h"

struct IPHeader 
{
    byte   
		iph_ihl: 4,
		iph_ver: 4;
    byte iph_tos;
    uint16_t iph_len;
    uint16_t iph_ident;
    byte iph_flags;
    byte iph_offset;
    byte iph_ttl;
    byte iph_protocol;
    uint16_t iph_xsum;
    uint32_t iph_sourceip;
    uint32_t iph_destip;
	byte *iph_data;
};

struct TCPHeader 
{
	uint16_t tcph_srcport;
	uint16_t tcph_destport;
	uint32_t tcph_seqnum;
	uint32_t tcph_acknum;
	byte 
		tcph_reserved: 4,
		tcph_offset: 4;
	byte	
		tcph_fin: 1, 
		tcph_syn: 1,    
		tcph_rst: 1, 
		tcph_psh: 1,
		tcph_ack: 1,
		tcph_urg: 1,
		tcph_reser: 2;
	uint16_t tcph_win;
	uint16_t tcph_xsum;
	uint16_t tcph_urgptr;
	byte *tcph_data;
};


struct UDPHeader 
{
	uint16_t udph_srcport;
	uint16_t udph_destport;
    uint16_t udph_len;
	uint16_t udph_xsum;
	byte *udph_data;
};