/* uuid32.h 
   2007-09-15 Last created by cheungmine.
   Partly rights reserved by cheungmine.
*/
#ifndef UUID32_H_INCLUDED
#define UUID32_H_INCLUDED

#include <stdlib.h>
#include <assert.h>
#include <string.h>
#include <memory.h>

#include "cdatatype.h"

typedef struct _timestamp_t
{
    BYTE    tm_sec;                /* Seconds after minute (0 每 59). */
    BYTE    tm_min;                /* Minutes after hour (0 每 59). */
    BYTE    tm_hour;            /* Hours after midnight (0 每 23). */
    BYTE    tm_mday;            /* Day of month (1 每 31). */
    BYTE    tm_mon;                /* Month (0 每 11; January = 0). */
    BYTE    tm_wday;            /* Day of week (0 每 6; Sunday = 0). */
    short    tm_year;            /* Year (current year minus 1900). */
    short    tm_yday;            /* Day of year (0 每 365; January 1 = 0). */
    long    tm_fraction;        /* Fraction little than 1 second */
} timestamp_t;

struct _uuid_t
{
    unsigned long    data1;
    unsigned short    data2;
    unsigned short    data3;
    unsigned char    data4[8];
};


/**
 * Checks whether the given string matches the UUID format.
 *    params:
 *     [in] uuid - the potential UUID string
 *    return 
 *     TRUE if the given string is a UUID, FALSE otherwise
 **/
BOOL is_uuid_string(const char *uuid);

/**
 * Generates a new UUID. The UUID is a time-based time 1 UUID.
 * A random per-process node identifier is used to avoid keeping global
 * state and maintaining inter-process synchronization.
 **/
void uuid_create(_uuid_t* uuid);

/**
 * Generates a new UUID string. The returned UUID is a time-based time 1 UUID.
 * A random per-process node identifier is used to avoid keeping global
 * state and maintaining inter-process synchronization.
 *  return UUID string (newly allocated)
 **/
char *uuid_create_string(void);

/**
 * Generates a name-based (type 3) UUID string from the given external
 * identifier. The special namespace UUID is used as the namespace of
 * the generated UUID.
 *  params
 *     [in] external - the external identifier
 *  return 
 *     UUID string (newly allocated)
 **/
void uuid_create_external(const char *external, _uuid_t* uuid);

/**
 * Translate a _uuid_t to a uuid string
 *  return UUID string
 **/
char *uuid_to_string(const _uuid_t* uuid);

/**
 * Get timestamp from a UUID
 **/
void uuid_to_timestamp(const _uuid_t* uuid, timestamp_t* time);


/**
 * Resurn a description of timestamp NOT including fraction
 **/
char* timestamp_to_string(const timestamp_t* time);

/**
 * Compare two UUID's lexically
 *    return
 *      -1   u1 is lexically before u2
 *     0   u1 is equal to u2
 *     1   u1 is lexically after u2
*/
int uuid_compare(const _uuid_t *u1, const _uuid_t *u2);

/**
 * Compare two UUID's temporally
 *    return
 *      -1   u1 is temporally before u2
 *     0   u1 is equal to u2
 *     1   u1 is temporally after u2
*/
int uuid_compare_time(const _uuid_t *u1, const _uuid_t *u2);


#endif        /* UUID32_H_INCLUDED */
