/* cdatatype.h 
   2008-09-15 Last created by cheungmine.
   All rights reserved by cheungmine.
*/
#ifndef CDATATYPE_H__
#define CDATATYPE_H__

/*============================================================================*/
typedef unsigned char uchar, byte, BYTE;

typedef unsigned short uint16, word_t, ushort;

typedef unsigned int uint, uint32, dword_t;

typedef unsigned long ulong;

#ifdef WIN32
typedef __int64 int64;
typedef unsigned __int64 uint64, qword_t;
//typedef unsigned int size_t;
#else
typedef long long int64;
typedef unsigned long long uint64, qword_t;
//typedef unsigned long size_t;
#endif

#ifndef BOOL
    #define BOOL  int
    #define TRUE  1
    #define FALSE 0
#endif

//#ifndef RESULT
//    #define RESULT  long
//    #define SUCCESS        0
//    #define ERROR        -1
//#endif

#define SIZE_BYTE    1
#define SIZE_SHORT    2
#define SIZE_INT    4
#define SIZE_FLT    4
#define SIZE_DBL    8
#define SIZE_WORD    2
#define SIZE_DWORD    4
#define SIZE_QWORD    8
#define SIZE_LINT    8
#define SIZE_INT64    8
#define SIZE_UUID    16

/*============================================================================*/
#endif    /*CDATATYPE_H__*/