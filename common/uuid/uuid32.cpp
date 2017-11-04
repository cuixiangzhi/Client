/* uuid32.c 
   2007-09-15 Last created by cheungmine.
   Partly rights reserved by cheungmine.
*/

#include <stdio.h>
#include <string.h>

#include <time.h>
#include <sys/types.h>
#include <sys/timeb.h>

#include "uuid32.h"
#include "md5.h"


#define MD5_LEN            16
#define UUID_LEN        36

/* microsecond per second. 1s=1000000us=1000000000ns*/
#define NSec100_Per_Sec        10000000
#define USec_Per_Sec        1000000
#define USec_Per_MSec        1000
#define NSec_Since_1582        ((uint64)(0x01B21DD213814000))


/*========================================================================================
                            Private Functions
========================================================================================*/
static BOOL isbigendian()
{
    int c = 1;
    return ( *((unsigned char *) &c) == 1 )? FALSE: TRUE;
};

static void swap_word( int size_bytes, void * ptr_word )
{
    int        i;
    unsigned char       temp;
    for( i=0; i < size_bytes/2; i++ )
    {
        temp = ((unsigned char *) ptr_word)[i];
        ((unsigned char *) ptr_word)[i] = ((unsigned char *) ptr_word)[size_bytes-i-1];
        ((unsigned char *) ptr_word)[size_bytes-i-1] = temp;
    }
};

static void write_word( unsigned char* stream, word_t val )
{
    memcpy(stream, &val, 2);
    if( isbigendian() ) swap_word( 2, stream );
};

static void write_dword( unsigned char* stream, dword_t val )
{
    memcpy(stream, &val, 4);
    if( isbigendian() ) swap_word( 4, stream );
};

static void  read_word( const unsigned char* stream, word_t* val )
{
    memcpy( val, stream, 2 );
    if( isbigendian() )    swap_word( 2, val );
};

static void  read_dword( const unsigned char* stream, dword_t* val )
{
    memcpy( val, stream, 4 );
    if( isbigendian() )    swap_word( 4, val );
};

static BOOL is_xdigit(char c)
{
    /* isxdigit returns a non-zero value if c is a hexadecimal digit (A ¨C F, a ¨C f, or 0 ¨C 9). */
    return ((c>='A'&&c<='F')||(c>='a'&&c<='f')||(c>='0'&&c<='9'))? TRUE : FALSE;
};


/* make a pseudorandom numbel based on current time*/
static int pseudo_rand()
{
#ifdef _USE_32BIT_TIME_T
    assert(0);
#endif

    struct timeb  timebuf;

#pragma warning(push)    /* C4996 */
#pragma warning( disable : 4996 )
    ftime(&timebuf);
#pragma warning(pop)    /* C4996 */
    
    srand((uint32) ((((uint32)timebuf.time&0xFFFF)+(uint32)timebuf.millitm)^(uint32)timebuf.millitm));

    return rand();
};


/*========================================================================================
                            Public Functions
========================================================================================*/

BOOL is_uuid_string(const char *uuid) 
{    
    static const char fmt[] = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx";
    int i;
    assert(uuid != NULL);
    for (i = 0; i < sizeof(fmt); i++)
        if (fmt[i] == 'x')
            if (!is_xdigit(uuid[i]))
                return FALSE;
        else if (uuid[i] != fmt[i])
            return FALSE;

    return TRUE;
}


/**
 * internal
 * ingroup uuid
 * The thread synchronization lock used to guarantee UUID uniqueness
 * for all the threads running within a process.
 */
void uuid_create(_uuid_t* u) 
{    
    static BOOL        initialized = FALSE;
    static int64    timestamp;
    static uint32    advance;
    static uint16    clockseq;
    static uint16    node_high;
    static uint32    node_low;
    int64            time;    /* unit of 100ns */
    uint16            nowseq;
    int                r;

    #ifdef _USE_32BIT_TIME_T
        assert(0);
    #endif

    struct timeb  tv;

    assert(u);

#pragma warning(push)    /* C4996 */
#pragma warning( disable : 4996 )
    ftime(&tv);
#pragma warning(pop)    /* C4996 */

    /* time is counter of 100ns time interval since Oct.15, 1582 (NOT 1852) */
    time = ((uint64) tv.time) * USec_Per_Sec + ((uint64) tv.millitm*USec_Per_MSec);
    time = time * 10 + NSec_Since_1582;

    if (!initialized) 
    {
        timestamp = time;
        advance = 0;

        r = pseudo_rand();

        clockseq = r >> 16;
        node_high = r | 0x0100;
        
        node_low = pseudo_rand();
        
        initialized = TRUE;
    } 
    else if (time < timestamp) 
    {
        timestamp = time;
        advance = 0;
        clockseq++;
    } 
    else if (time == timestamp) 
    {
        advance++;
        time += advance;
    } 
    else 
    {
        timestamp = time;
        advance = 0;
    }
    nowseq = clockseq;

    assert(u);
    u->data1 = (dword_t) time;
    u->data2 = (word_t) ((time >> 32) & 0xffff);
    u->data3 = (word_t) (((time >> 48) & 0x0ffff) | 0x1000);
    write_word(&(u->data4[6]), (word_t) ((nowseq & 0x3fff) | 0x8000));    
    write_word(&(u->data4[4]), (word_t) (node_high));                    
    write_dword(&(u->data4[0]), (dword_t) (node_low));            
}

/**
 * internal
 * ingroup uuid
 * The thread synchronization lock used to guarantee UUID uniqueness
 * for all the threads running within a process.
 */
char *uuid_create_string(void) 
{
    _uuid_t  u;
    uuid_create(&u);
    return uuid_to_string(&u);
}

char *uuid_to_string(const _uuid_t*  u)
{
    static char uuid_str[UUID_LEN+1];
    ushort a,b;
    uint32  c;
    read_word(&(u->data4[6]), &a);
    read_word(&(u->data4[4]), &b);
    read_dword(&(u->data4[0]), &c);

#pragma warning(push)    /* C4996 */
#pragma warning( disable : 4996 )
    sprintf(uuid_str, "%08lx-%04x-%04x-%04x-%04x%08lx", 
                u->data1,
                u->data2,
                u->data3,
                a, b, c);
#pragma warning(pop)    /* C4996 */
    return uuid_str;
}

/**
 * internal
 * ingroup uuid
 * The predefined namespace UUID. Expressed in binary format
 * to avoid unnecessary conversion when generating name based UUIDs.
 */
static const unsigned char namespace_uuid[] = {
        0x9c, 0xfb, 0xd9, 0x1f, 0x11, 0x72, 0x4a, 0xf6,
        0xbd, 0xcb, 0x9f, 0x34, 0xe4, 0x6f, 0xa0, 0xfb
};

void  uuid_create_external(const char *external, _uuid_t* u) 
{
    MD5_CTX md5;
    unsigned char uuid[16];    
    
    assert(external != NULL);

    MD5_init(&md5);
    MD5_update(&md5, namespace_uuid, sizeof(namespace_uuid));
    MD5_update(&md5, (unsigned char *) external, (unsigned int) strlen(external));
    MD5_fini(uuid, &md5);

    u->data1 = (dword_t) (uuid[0] << 24 | uuid[1] << 16 | uuid[2] << 8 | uuid[3]);
    u->data2 = (word_t)  (uuid[4] << 8 | uuid[5]);
    u->data3 = (word_t)  (((uuid[6] & 0x0f) | 0x30) << 8 | uuid[7]);    
    
    /* BYTE 6-7 */
    write_word(&(u->data4[6]), (word_t) (((uuid[8] & 0x3f) | 0x80) << 8 | uuid[9]));        
    /* BYTE 4-5 */
    write_word(&(u->data4[4]), (word_t) (uuid[10] << 8 | uuid[11]));                        
    /* BYTE 0-3 */
    write_dword(&(u->data4[0]), (dword_t) (uuid[12] << 24 | uuid[13] << 16 | uuid[14] << 8 | uuid[15]));
}

/**
 * Get timestamp from a UUID
 **/
void uuid_to_timestamp(const _uuid_t* u, timestamp_t* t)
{
    time_t   time, t2, t3;
    struct  tm*  p;
    assert(u);

    t2 = u->data2;
    t3 = u->data3;

    time = u->data1 + (t2<<32) + ((t3&0x0fff)<<48);        /* 100ns */
    time -= NSec_Since_1582;

    t->tm_fraction = (long)(time%NSec100_Per_Sec);
    
    time /= 10;
    time /= USec_Per_Sec; 
    
#pragma warning(push)    /* C4996 */
#pragma warning( disable : 4996 )
    p = localtime(&time);
#pragma warning(pop)    /* C4996 */
    
    t->tm_hour = p->tm_hour;
    t->tm_mday = p->tm_mday;
    t->tm_min = p->tm_min;
    t->tm_mon = p->tm_mon;
    t->tm_sec = p->tm_sec;
    t->tm_wday = p->tm_wday;
    t->tm_yday = p->tm_yday;
    t->tm_year = p->tm_year;
}

char* timestamp_to_string(const timestamp_t* time)
{
    struct tm t;
    t.tm_hour = time->tm_hour;
    t.tm_mday = time->tm_mday;
    t.tm_min = time->tm_min;
    t.tm_mon = time->tm_mon;
    t.tm_sec = time->tm_sec;
    t.tm_wday = time->tm_wday;
    t.tm_yday = time->tm_yday;
    t.tm_year = time->tm_year;

#pragma warning(push)    /* C4996 */
#pragma warning( disable : 4996 )
    return asctime(&t);
#pragma warning(pop)    /* C4996 */
}



/**
 * Compare two UUID's lexically
 *    return
 *      -1   u1 is lexically before u2
 *     0   u1 is equal to u2
 *     1   u1 is lexically after u2
*/
int uuid_compare(const _uuid_t *u1, const _uuid_t *u2)
{
    int i;

#define CHECK_COMP(f1, f2)  if ((f1) != (f2)) return ((f1) < (f2) ? -1 : 1);
    
    CHECK_COMP(u1->data1, u2->data1);
    CHECK_COMP(u1->data2, u2->data2);
    CHECK_COMP(u1->data3, u2->data3);

    for(i=0; i<8; i++)
        CHECK_COMP(u1->data4[i], u1->data4[i]);

#undef CHECK_COMP

    return 0;
}

/**
 * Compare two UUID's temporally
 *    return
 *      -1   u1 is temporally before u2
 *     0   u1 is equal to u2
 *     1   u1 is temporally after u2
*/
int uuid_compare_time(const _uuid_t *u1, const _uuid_t *u2)
{    
#define CHECK_COMP(f1, f2)  if ((f1) != (f2)) return ((f1) < (f2) ? -1 : 1);
    
    CHECK_COMP(u1->data1, u2->data1);
    CHECK_COMP(u1->data2, u2->data2);
    CHECK_COMP(u1->data3, u2->data3);

#undef CHECK_COMP

    return 0;
}