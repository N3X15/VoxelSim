/*************************************************************************
 *                                                                       *
 * ODER's Utilities Library. Copyright (C) 2008 Oleh Derevenko.          *
 * All rights reserved.  e-mail: odar@eleks.com (change all "a" to "e")  *
 *                                                                       *
 * This library is free software; you can redistribute it and/or         *
 * modify it under the terms of EITHER:                                  *
 *   (1) The GNU Lesser General Public License as published by the Free  *
 *       Software Foundation; either version 3 of the License, or (at    *
 *       your option) any later version. The text of the GNU Lesser      *
 *       General Public License is included with this library in the     *
 *       file LICENSE-LESSER.TXT. Since LGPL is the extension of GPL     *
 *       the text of GNU General Public License is also provided for     *
 *       your information in file LICENSE.TXT.                           *
 *   (2) The BSD-style license that is included with this library in     *
 *       the file LICENSE-BSD.TXT.                                       *
 *   (3) The zlib/libpng license that is included with this library in   *
 *       the file LICENSE-ZLIB.TXT                                       *
 *                                                                       *
 * This library is distributed WITHOUT ANY WARRANTY, including implied   *
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.      *
 * See the files LICENSE.TXT and LICENSE-LESSER.TXT or LICENSE-BSD.TXT   *
 * or LICENSE-ZLIB.TXT for more details.                                 *
 *                                                                       *
 *************************************************************************/

#ifndef __OU_ATOMIC_H_INCLUDED
#define __OU_ATOMIC_H_INCLUDED


#include <ou/inttypes.h>
#include <ou/namespace.h>
#include <ou/platform.h>


BEGIN_NAMESPACE_OU();


//////////////////////////////////////////////////////////////////////////
// Windows implementation 

#if _OU_TARGET_OS == _OU_TARGET_OS_WINDOWS


END_NAMESPACE_OU();


#include <windows.h>
#include <stddef.h>


BEGIN_NAMESPACE_OU();


typedef LONG atomicord32;
typedef PVOID atomicptr;


#if _OU_COMPILER == _OU_COMPILER_MSVC && _OU_COMPILER_VERSION == _OU_COMPILER_VERSION_MSVC1998

#define __ou_intlck_value_t LONG
#define __ou_intlck_target_t LPLONG
#define __ou_xchgadd_target_t LPLONG
#define __ou_cmpxchg_value_t PVOID
#define __ou_cmpxchg_target_t PVOID *


#elif _OU_COMPILER == _OU_COMPILER_GCC

#define __ou_intlck_value_t LONG
#define __ou_intlck_target_t LPLONG
#define __ou_xchgadd_target_t LPLONG
#define __ou_cmpxchg_value_t LONG
#define __ou_cmpxchg_target_t LPLONG


#else // other compilers

#define __ou_intlck_value_t atomicord32
#define __ou_intlck_target_t volatile atomicord32 *
#define __ou_xchgadd_target_t LPLONG
#define __ou_cmpxchg_value_t atomicord32
#define __ou_cmpxchg_target_t volatile atomicord32 *


#endif // #if _OU_COMPILER == _OU_COMPILER_GCC


#define __OU_ATOMIC_ORD32_FUNCTIONS_DEFINED

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicIncrement(volatile atomicord32 *paoDestination)
{
	return ::InterlockedIncrement((__ou_intlck_target_t)paoDestination);
}

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicDecrement(volatile atomicord32 *paoDestination)
{
	return ::InterlockedDecrement((__ou_intlck_target_t)paoDestination);
}


static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicExchange(volatile atomicord32 *paoDestination, atomicord32 aoExchange)
{
	return ::InterlockedExchange((__ou_intlck_target_t)paoDestination, aoExchange);
}

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicExchangeAdd(volatile atomicord32 *paoDestination, atomicord32 aoAddend)
{
	return ::InterlockedExchangeAdd((__ou_xchgadd_target_t)paoDestination, aoAddend);
}

static _OU_ALWAYSINLINE_PRE bool _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*bool */AtomicCompareExchange(volatile atomicord32 *paoDestination, atomicord32 aoComparand, atomicord32 aoExchange)
{
	return (aoComparand == (atomicord32)::InterlockedCompareExchange((__ou_cmpxchg_target_t)paoDestination, (__ou_cmpxchg_value_t)aoExchange, (__ou_cmpxchg_value_t)aoComparand));
}


#define __OU_ATOMIC_BIT_FUNCTIONS_DEFINED

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicAnd(volatile atomicord32 *paoDestination, atomicord32 aoBitMask)
{
	atomicord32 aoOldValue = *paoDestination;
	
    while (true)
	{
        atomicord32 aoNewValue = (atomicord32)::InterlockedCompareExchange((__ou_cmpxchg_target_t)paoDestination, (__ou_cmpxchg_value_t)(aoOldValue & aoBitMask), (__ou_cmpxchg_value_t)aoOldValue);

		if (aoNewValue == aoOldValue)
		{
			break;
		}

		aoOldValue = aoNewValue;
    }
	
    return aoOldValue;
}

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicOr(volatile atomicord32 *paoDestination, atomicord32 aoBitMask)
{
	atomicord32 aoOldValue = *paoDestination;
	
    while (true)
	{
        atomicord32 aoNewValue = (atomicord32)::InterlockedCompareExchange((__ou_cmpxchg_target_t)paoDestination, (__ou_cmpxchg_value_t)(aoOldValue | aoBitMask), (__ou_cmpxchg_value_t)aoOldValue);
		
		if (aoNewValue == aoOldValue)
		{
			break;
		}
		
		aoOldValue = aoNewValue;
    }
	
    return aoOldValue;
}

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicXor(volatile atomicord32 *paoDestination, atomicord32 aoBitMask)
{
	atomicord32 aoOldValue = *paoDestination;
	
    while (true)
	{
        atomicord32 aoNewValue = (atomicord32)::InterlockedCompareExchange((__ou_cmpxchg_target_t)paoDestination, (__ou_cmpxchg_value_t)(aoOldValue ^ aoBitMask), (__ou_cmpxchg_value_t)aoOldValue);
		
		if (aoNewValue == aoOldValue)
		{
			break;
		}
		
		aoOldValue = aoNewValue;
    }
	
    return aoOldValue;
}


#define __OU_ATOMIC_PTR_FUNCTIONS_DEFINED

static _OU_ALWAYSINLINE_PRE atomicptr _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicptr */AtomicExchangePointer(volatile atomicptr *papDestination, atomicptr apExchange)
{
#if _OU_TARGET_BITS == _OU_TARGET_BITS_32

	return (atomicptr)(ptrdiff_t)::InterlockedExchange((__ou_intlck_target_t)papDestination, (__ou_intlck_value_t)(ptrdiff_t)apExchange);
	

#else // #if _OU_TARGET_BITS == _OU_TARGET_BITS_64

	return ::InterlockedExchangePointer(papDestination, apExchange);
	

#endif // #if _OU_TARGET_BITS == ...
}

static _OU_ALWAYSINLINE_PRE bool _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*bool */AtomicCompareExchangePointer(volatile atomicptr *papDestination, atomicptr apComparand, atomicptr apExchange)
{
#if _OU_TARGET_BITS == _OU_TARGET_BITS_32
	
	return (apComparand == (atomicptr)(ptrdiff_t)::InterlockedCompareExchange((__ou_cmpxchg_target_t)papDestination, (__ou_cmpxchg_value_t)(ptrdiff_t)apExchange, (__ou_cmpxchg_value_t)(ptrdiff_t)apComparand));

	
#else // #if !defined(__OU_ATOMIC_WINDOWS_OLD_STYLE_PARAMS)
	
	return (apComparand == ::InterlockedCompareExchangePointer(papDestination, apExchange, apComparand));
	
	
#endif // #if !defined(__OU_ATOMIC_WINDOWS_OLD_STYLE_PARAMS)
}


#undef __ou_intlck_value_t
#undef __ou_intlck_target_t
#undef __ou_xchgadd_target_t
#undef __ou_cmpxchg_value_t
#undef __ou_cmpxchg_target_t


#endif // #if _OU_TARGET_OS == _OU_TARGET_OS_WINDOWS


//////////////////////////////////////////////////////////////////////////
// QNX implementation 

#if _OU_TARGET_OS == _OU_TARGET_OS_QNX

END_NAMESPACE_OU();


#include <atomic.h>
#include _NTO_CPU_HDR_(smpxchg.h)


BEGIN_NAMESPACE_OU();

typedef unsigned int atomicord32;
typedef void *atomicptr;


#define __OU_ATOMIC_ORD32_FUNCTIONS_DEFINED

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicIncrement(volatile atomicord32 *paoDestination)
{
	return (atomic_add_value(paoDestination, 1U) + 1U);
}

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicDecrement(volatile atomicord32 *paoDestination)
{
	return (atomic_sub_value(paoDestination, 1U) - 1U);
}


static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicExchange(volatile atomicord32 *paoDestination, atomicord32 aoExchange)
{
	return _smp_xchg(paoDestination, aoExchange);
}

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicExchangeAdd(volatile atomicord32 *paoDestination, atomicord32 aoAddend)
{
	return atomic_add_value(paoDestination, aoAddend);
}

static _OU_ALWAYSINLINE_PRE bool _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*bool */AtomicCompareExchange(volatile atomicord32 *paoDestination, atomicord32 aoComparand, atomicord32 aoExchange)
{
	return (aoComparand == (atomicord32)_smp_cmpxchg(paoDestination, aoComparand, aoExchange));
}


#define __OU_ATOMIC_BIT_FUNCTIONS_DEFINED

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicAnd(volatile atomicord32 *paoDestination, atomicord32 aoBitMask)
{
	return atomic_clr_value(paoDestination, ~aoBitMask);
}

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicOr(volatile atomicord32 *paoDestination, atomicord32 aoBitMask)
{
	return atomic_set_value(paoDestination, aoBitMask);
}

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicXor(volatile atomicord32 *paoDestination, atomicord32 aoBitMask)
{
	return atomic_toggle_value(paoDestination, aoBitMask);
}


#define __OU_ATOMIC_ORD32_NORESULT_FUNCTIONS_DEFINED

static _OU_ALWAYSINLINE_PRE void _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*void */AtomicIncrementNoResult(volatile atomicord32 *paoDestination)
{
	atomic_add(paoDestination, 1U);
}

static _OU_ALWAYSINLINE_PRE void _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*void */AtomicDecrementNoResult(volatile atomicord32 *paoDestination)
{
	atomic_sub(paoDestination, 1U);
}

static _OU_ALWAYSINLINE_PRE void _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*void */AtomicExchangeAddNoResult(volatile atomicord32 *paoDestination, atomicord32 aoAddend)
{
	atomic_add(paoDestination, aoAddend);
}

static _OU_ALWAYSINLINE_PRE void _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*void */AtomicAndNoResult(volatile atomicord32 *paoDestination, atomicord32 aoBitMask)
{
	atomic_clr(paoDestination, ~aoBitMask);
}

static _OU_ALWAYSINLINE_PRE void _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*void */AtomicOrNoResult(volatile atomicord32 *paoDestination, atomicord32 aoBitMask)
{
	atomic_set(paoDestination, aoBitMask);
}

static _OU_ALWAYSINLINE_PRE void _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*void */AtomicXorNoResult(volatile atomicord32 *paoDestination, atomicord32 aoBitMask)
{
	atomic_toggle(paoDestination, aoBitMask);
}


#endif // #if _OU_TARGET_OS == _OU_TARGET_OS_QNX


//////////////////////////////////////////////////////////////////////////
// Mac implementation

#if _OU_TARGET_OS == _OU_TARGET_OS_MAC

#if MAC_OS_X_VERSION >= 1040


END_NAMESPACE_OU();


#include <libkern/OSAtomic.h>


BEGIN_NAMESPACE_OU();


typedef int32_t atomicord32;
typedef void *atomicptr;


#define __ou_bitmsk_target_t volatile uint32_t *


#define __OU_ATOMIC_ORD32_FUNCTIONS_DEFINED

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicIncrement(volatile atomicord32 *paoDestination)
{
	return OSAtomicIncrement32Barrier(paoDestination);
}

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicDecrement(volatile atomicord32 *paoDestination)
{
	return OSAtomicDecrement32Barrier(paoDestination);
}


static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicExchange(volatile atomicord32 *paoDestination, atomicord32 aoExchange)
{
	atomicord32 aoOldValue = *paoDestination;

	/*
	 *	Implementation Note:
	 *	It is safe to use compare-and-swap without memory barrier for subsequent attempts
	 *	because current thread had already had a barrier and does not have any additional
	 *	memory access until function exit. On the other hand it is expected that other 
	 *	threads will be using this API set for manipulations with paoDestination as well
	 *	and hence will not issue writes after/without memory barrier.
	 */
	for (bool bSwapExecuted = OSAtomicCompareAndSwap32Barrier(aoOldValue, aoExchange, paoDestination);
		!bSwapExecuted; bSwapExecuted = OSAtomicCompareAndSwap32(aoOldValue, aoExchange, paoDestination))
	{
		aoOldValue = *paoDestination;
	}
	
	return aoOldValue;
}

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicExchangeAdd(volatile atomicord32 *paoDestination, atomicord32 aoAddend)
{
	return (OSAtomicAdd32Barrier(aoAddend, paoDestination) - aoAddend);
}

static _OU_ALWAYSINLINE_PRE bool _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*bool */AtomicCompareExchange(volatile atomicord32 *paoDestination, atomicord32 aoComparand, atomicord32 aoExchange)
{
	return OSAtomicCompareAndSwap32Barrier(aoComparand, aoExchange, paoDestination);
}


#if MAC_OS_X_VERSION >= 1050

#define __OU_ATOMIC_BIT_FUNCTIONS_DEFINED

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicAnd(volatile atomicord32 *paoDestination, atomicord32 aoBitMask)
{
	return OSAtomicAnd32OrigBarrier(aoBitMask, (__ou_bitmsk_target_t)paoDestination);
}

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicOr(volatile atomicord32 *paoDestination, atomicord32 aoBitMask)
{
	return OSAtomicOr32OrigBarrier(aoBitMask, (__ou_bitmsk_target_t)paoDestination);
}

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicXor(volatile atomicord32 *paoDestination, atomicord32 aoBitMask)
{
	return OSAtomicXor32OrigBarrier(aoBitMask, (__ou_bitmsk_target_t)paoDestination);
}


#else // #if MAC_OS_X_VERSION < 1050 (...&& MAC_OS_X_VERSION >= 1040)

#define __OU_ATOMIC_BIT_FUNCTIONS_DEFINED

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicAnd(volatile atomicord32 *paoDestination, atomicord32 aoBitMask)
{
	atomicord32 aoOldValue = *paoDestination;

	/*
	 *	Implementation Note:
	 *	It is safe to use compare-and-swap without memory barrier for subsequent attempts
	 *	because current thread had already had a barrier and does not have any additional
	 *	memory access until function exit. On the other hand it is expected that other 
	 *	threads will be using this API set for manipulations with paoDestination as well
	 *	and hence will not issue writes after/without memory barrier.
	 */
	for (bool bSwapExecuted = OSAtomicCompareAndSwap32Barrier(aoOldValue, (aoOldValue & aoBitMask), paoDestination);
		!bSwapExecuted; bSwapExecuted = OSAtomicCompareAndSwap32(aoOldValue, (aoOldValue & aoBitMask), paoDestination))
	{
		aoOldValue = *paoDestination;
	}
	
	return aoOldValue;
}

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicOr(volatile atomicord32 *paoDestination, atomicord32 aoBitMask)
{
	atomicord32 aoOldValue = *paoDestination;

	/*
	 *	Implementation Note:
	 *	It is safe to use compare-and-swap without memory barrier for subsequent attempts
	 *	because current thread had already had a barrier and does not have any additional
	 *	memory access until function exit. On the other hand it is expected that other 
	 *	threads will be using this API set for manipulations with paoDestination as well
	 *	and hence will not issue writes after/without memory barrier.
	 */
	for (bool bSwapExecuted = OSAtomicCompareAndSwap32Barrier(aoOldValue, (aoOldValue | aoBitMask), paoDestination);
		!bSwapExecuted; bSwapExecuted = OSAtomicCompareAndSwap32(aoOldValue, (aoOldValue | aoBitMask), paoDestination))
	{
		aoOldValue = *paoDestination;
	}
	
	return aoOldValue;
}

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicXor(volatile atomicord32 *paoDestination, atomicord32 aoBitMask)
{
	return (OSAtomicXor32Barrier(aoBitMask, (__ou_bitmsk_target_t)paoDestination) ^ aoBitMask);
}


#endif // #if MAC_OS_X_VERSION < 1050 (...&& MAC_OS_X_VERSION >= 1040)


#if MAC_OS_X_VERSION >= 1050

#define __OU_ATOMIC_PTR_FUNCTIONS_DEFINED

static _OU_ALWAYSINLINE_PRE atomicptr _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicptr */AtomicExchangePointer(volatile atomicptr *papDestination, atomicptr apExchange)
{
	atomicptr apOldValue = *papDestination;

	/*
	 *	Implementation Note:
	 *	It is safe to use compare-and-swap without memory barrier for subsequent attempts
	 *	because current thread had already had a barrier and does not have any additional
	 *	memory access until function exit. On the other hand it is expected that other 
	 *	threads will be using this API set for manipulations with papDestination as well
	 *	and hence will not issue writes after/without memory barrier.
	 */
	for (bool bSwapExecuted = OSAtomicCompareAndSwapPtrBarrier(apOldValue, apExchange, papDestination);
		!bSwapExecuted; bSwapExecuted = OSAtomicCompareAndSwapPtr(apOldValue, apExchange, papDestination))
	{
		apOldValue = *papDestination;
	}
	
	return apOldValue;
}

static _OU_ALWAYSINLINE_PRE bool _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*bool */AtomicCompareExchangePointer(volatile atomicptr *papDestination, atomicptr apComparand, atomicptr apExchange)
{
	return OSAtomicCompareAndSwapPtrBarrier(apComparand, apExchange, papDestination);
}


#endif // #if MAC_OS_X_VERSION >= 1050


#define __OU_ATOMIC_ORD32_NORESULT_FUNCTIONS_DEFINED

static _OU_ALWAYSINLINE_PRE void _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*void */AtomicIncrementNoResult(volatile atomicord32 *paoDestination)
{
	OSAtomicIncrement32Barrier(paoDestination);
}

static _OU_ALWAYSINLINE_PRE void _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*void */AtomicDecrementNoResult(volatile atomicord32 *paoDestination)
{
	OSAtomicDecrement32Barrier(paoDestination);
}

static _OU_ALWAYSINLINE_PRE void _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*void */AtomicExchangeAddNoResult(volatile atomicord32 *paoDestination, atomicord32 aoAddend)
{
	OSAtomicAdd32Barrier(aoAddend, paoDestination);
}

static _OU_ALWAYSINLINE_PRE void _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*void */AtomicAndNoResult(volatile atomicord32 *paoDestination, atomicord32 aoBitMask)
{
	OSAtomicAnd32Barrier(aoBitMask, (__ou_bitmsk_target_t)paoDestination);
}

static _OU_ALWAYSINLINE_PRE void _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*void */AtomicOrNoResult(volatile atomicord32 *paoDestination, atomicord32 aoBitMask)
{
	OSAtomicOr32Barrier(aoBitMask, (__ou_bitmsk_target_t)paoDestination);
}

static _OU_ALWAYSINLINE_PRE void _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*void */AtomicXorNoResult(volatile atomicord32 *paoDestination, atomicord32 aoBitMask)
{
	OSAtomicXor32Barrier(aoBitMask, (__ou_bitmsk_target_t)paoDestination);
}


#endif // #if MAC_OS_X_VERSION >= 1040


#undef __ou_bitmsk_target_t

#endif // #if _OU_TARGET_OS == _OU_TARGET_OS_MAC


//////////////////////////////////////////////////////////////////////////
// AIX implementation

#if _OU_TARGET_OS == _OU_TARGET_OS_AIX


END_NAMESPACE_OU();


#include <sys/atomic_op.h>


BEGIN_NAMESPACE_OU();


typedef int atomicord32;
typedef void *atomicptr;


#define __OU_ATOMIC_ORD32_FUNCTIONS_DEFINED

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicIncrement(volatile atomicord32 *paoDestination)
{
	return (fetch_and_add((atomic_p)paoDestination, 1) + 1);
}

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicDecrement(volatile atomicord32 *paoDestination)
{
	return (fetch_and_add((atomic_p)paoDestination, -1) - 1);
}


static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicExchange(volatile atomicord32 *paoDestination, atomicord32 aoExchange)
{
	atomicord32 aoOldValue = *paoDestination;

	while (!compare_and_swap((atomic_p)paoDestination, &aoOldValue, aoExchange))
	{
		// Do nothing
	}

	return aoOldValue;
}

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicExchangeAdd(volatile atomicord32 *paoDestination, atomicord32 aoAddend)
{
	return fetch_and_add((atomic_p)paoDestination, aoAddend);
}

static _OU_ALWAYSINLINE_PRE bool _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*bool */AtomicCompareExchange(volatile atomicord32 *paoDestination, atomicord32 aoComparand, atomicord32 aoExchange)
{
	atomicord32 aoOldValue = aoComparand;

	return compare_and_swap((atomic_p)paoDestination, &aoOldValue, aoExchange);
}


#define __OU_ATOMIC_BIT_FUNCTIONS_DEFINED

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicAnd(volatile atomicord32 *paoDestination, atomicord32 aoBitMask)
{
	return fetch_and_and((atomic_p)paoDestination, aoBitMask);
}

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicOr(volatile atomicord32 *paoDestination, atomicord32 aoBitMask)
{
	return fetch_and_or((atomic_p)paoDestination, aoBitMask);
}

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicXor(volatile atomicord32 *paoDestination, atomicord32 aoBitMask)
{
	volatile atomicord32 aoOldValue = *paoDestination;
	
	while (!compare_and_swap((atomic_p)paoDestination, &aoOldValue, aoOldValue ^ aoBitMask))
	{
		// Do nothing
	}
	
	return aoOldValue;
}


#if _OU_TARGET_BITS == _OU_TARGET_BITS_64 // Otherwise functions will be forwarded to ord32 further in this file

#define __OU_ATOMIC_PTR_FUNCTIONS_DEFINED

static _OU_ALWAYSINLINE_PRE atomicptr _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicptr */AtomicExchangePointer(volatile atomicptr *papDestination, atomicptr apExchange)
{
	long liOldValue = (long)*papDestination;
	
	while (!compare_and_swaplp((atomic_l)papDestination, &liOldValue, (long)apExchange))
	{
		// Do nothing
	}
	
	return liOldValue;
}

static _OU_ALWAYSINLINE_PRE bool _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*bool */AtomicCompareExchangePointer(volatile atomicptr *papDestination, atomicptr apComparand, atomicptr apExchange)
{
	long liOldValue = (long)apComparand;
	
	return compare_and_swaplp((atomic_l)papDestination, &liOldValue, (long)apExchange);
}


#endif // #if _OU_TARGET_BITS == _OU_TARGET_BITS_64


#endif // #if _OU_TARGET_OS == _OU_TARGET_OS_AIX


//////////////////////////////////////////////////////////////////////////
// SunOS implementation

#if _OU_TARGET_OS == _OU_TARGET_OS_SUNOS


END_NAMESPACE_OU();


#include <atomic.h>


BEGIN_NAMESPACE_OU();


typedef uint32_t atomicord32;
typedef void *atomicptr;


#define __OU_ATOMIC_ORD32_FUNCTIONS_DEFINED

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicIncrement(volatile atomicord32 *paoDestination)
{
	return atomic_inc_32_nv(paoDestination);
}

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicDecrement(volatile atomicord32 *paoDestination)
{
	return atomic_dec_32_nv(paoDestination);
}


static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicExchange(volatile atomicord32 *paoDestination, atomicord32 aoExchange)
{
	return atomic_swap_32(paoDestination, aoExchange);
}

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicExchangeAdd(volatile atomicord32 *paoDestination, atomicord32 aoAddend)
{
	return (atomic_add_32_nv(paoDestination, aoAddend) - aoAddend);
}

static _OU_ALWAYSINLINE_PRE bool _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*bool */AtomicCompareExchange(volatile atomicord32 *paoDestination, atomicord32 aoComparand, atomicord32 aoExchange)
{
	return (aoComparand == atomic_cas_32(paoDestination, aoComparand, aoExchange));
}


#define __OU_ATOMIC_BIT_FUNCTIONS_DEFINED

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicAnd(volatile atomicord32 *paoDestination, atomicord32 aoBitMask)
{
	atomicord32 aoOldValue = *paoDestination;

	while (true)
	{
		atomicord32 aoNewValue = atomic_cas_32(paoDestination, aoOldValue, aoOldValue & aoBitMask);

		if (aoNewValue == aoOldValue)
		{
			break;
		}

		aoOldValue = aoNewValue;
	}

	return aoOldValue;
}

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicOr(volatile atomicord32 *paoDestination, atomicord32 aoBitMask)
{
	atomicord32 aoOldValue = *paoDestination;
	
	while (true)
	{
		atomicord32 aoNewValue = atomic_cas_32(paoDestination, aoOldValue, aoOldValue | aoBitMask);
		
		if (aoNewValue == aoOldValue)
		{
			break;
		}
		
		aoOldValue = aoNewValue;
	}
	
	return aoOldValue;
}

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicXor(volatile atomicord32 *paoDestination, atomicord32 aoBitMask)
{
	atomicord32 aoOldValue = *paoDestination;
	
	while (true)
	{
		atomicord32 aoNewValue = atomic_cas_32(paoDestination, aoOldValue, aoOldValue ^ aoBitMask);
		
		if (aoNewValue == aoOldValue)
		{
			break;
		}
		
		aoOldValue = aoNewValue;
	}
	
	return aoOldValue;
}


#define __OU_ATOMIC_PTR_FUNCTIONS_DEFINED

static _OU_ALWAYSINLINE_PRE atomicptr _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicptr */AtomicExchangePointer(volatile atomicptr *papDestination, atomicptr apExchange)
{
	return atomic_swap_ptr(papDestination, apExchange);
}

static _OU_ALWAYSINLINE_PRE bool _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*bool */AtomicCompareExchangePointer(volatile atomicptr *papDestination, atomicptr apComparand, atomicptr apExchange)
{
	return (apComparand == atomic_cas_ptr(papDestination, apComparand, apExchange));
}


#define __OU_ATOMIC_ORD32_NORESULT_FUNCTIONS_DEFINED

static _OU_ALWAYSINLINE_PRE void _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*void */AtomicIncrementNoResult(volatile atomicord32 *paoDestination)
{
	atomic_inc_32(paoDestination);
}

static _OU_ALWAYSINLINE_PRE void _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*void */AtomicDecrementNoResult(volatile atomicord32 *paoDestination)
{
	atomic_dec_32(paoDestination);
}

static _OU_ALWAYSINLINE_PRE void _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*void */AtomicExchangeAddNoResult(volatile atomicord32 *paoDestination, atomicord32 aoAddend)
{
	atomic_add_32(paoDestination, aoAddend);
}

static _OU_ALWAYSINLINE_PRE void _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*void */AtomicAndNoResult(volatile atomicord32 *paoDestination, atomicord32 aoBitMask)
{
	atomic_and_32(paoDestination, aoBitMask);
}

static _OU_ALWAYSINLINE_PRE void _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*void */AtomicOrNoResult(volatile atomicord32 *paoDestination, atomicord32 aoBitMask)
{
	atomic_or_32(paoDestination, aoBitMask);
}

static _OU_ALWAYSINLINE_PRE void _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*void */AtomicXorNoResult(volatile atomicord32 *paoDestination, atomicord32 aoBitMask)
{
	AtomicXor(paoDestination, aoBitMask);
}


#endif // #if _OU_TARGET_OS == _OU_TARGET_OS_SUNOS


//////////////////////////////////////////////////////////////////////////
// Generic UNIX implementation

#if _OU_TARGET_OS == _OU_TARGET_OS_GENUNIX

// No atomic functions for generic UNIX

// x86 assembler implementation for i486 must be engaged explicitly
#if defined(_OU_ATOMIC_USE_X86_ASSEMBLER)


typedef uint32ou atomicord32;
typedef void *atomicptr;


struct _ou_atomic_CLargeStruct
{ 
	unsigned int	m_uiData[32];
};


#define __OU_ATOMIC_ORD32_FUNCTIONS_DEFINED


static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicIncrement(volatile atomicord32 *paoDestination)
{
	register atomicord32 aoResult = 1;

	asm volatile (
		"lock; xaddl %2, %0;"
		: "=m" (*(volatile _ou_atomic_CLargeStruct *)paoDestination), "=a" (aoResult)
		: "1" (aoResult)
		: "memory");

	return aoResult + 1;
}

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicDecrement(volatile atomicord32 *paoDestination)
{
	register atomicord32 aoResult = (atomicord32)(-1);

	asm volatile (
		"lock; xaddl %2, %0;"
		: "=m" (*(volatile _ou_atomic_CLargeStruct *)paoDestination), "=a" (aoResult)
		: "1" (aoResult)
		: "memory");

	return aoResult - 1;
}


static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicExchange(volatile atomicord32 *paoDestination, atomicord32 aoExchange)
{
	register atomicord32 aoResult;

	asm volatile (
		"xchg %2, %0;"
		: "=m" (*(volatile _ou_atomic_CLargeStruct *)paoDestination), "=a" (aoResult)
		: "1" (aoExchange)
		: "memory");

	return aoResult;
}

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicExchangeAdd(volatile atomicord32 *paoDestination, atomicord32 aoAddend)
{
	register atomicord32 aoResult;

	asm volatile (
		"lock; xaddl %2, %0;"
		: "=m" (*(volatile _ou_atomic_CLargeStruct *)paoDestination), "=a" (aoResult)
		: "1" (aoAddend)
		: "memory");

	return aoResult;
}

static _OU_ALWAYSINLINE_PRE bool _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*bool */AtomicCompareExchange(volatile atomicord32 *paoDestination, atomicord32 aoComparand, atomicord32 aoExchange)
{
	register bool bResult;

	asm volatile (
		"lock; cmpxchgl %3, %0;"
		"setzb %1;"
		: "=m" (*(volatile _ou_atomic_CLargeStruct *)paoDestination), "=a" (bResult)
		: "a" (aoComparand), "r" (aoExchange)
		: "memory");

	return bResult;
}


#define __OU_ATOMIC_BIT_FUNCTIONS_DEFINED

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicAnd(volatile atomicord32 *paoDestination, atomicord32 aoBitMask)
{
	register atomicord32 aoResult;
	register atomicord32 aoExchange;

	asm volatile (
	"0:;"
		"movl  %4, %2;"
		"andl  %3, %2;"
		"lock; cmpxchgl %2, %0;"
		"jnz   0;"
		: "=m" (*(volatile _ou_atomic_CLargeStruct *)paoDestination), "=a" (aoResult), "=r" (aoExchange)
		: "a" (*paoDestination), "g" (aoBitMask), "m" (*paoDestination)
		: "memory");

	return aoResult;
}

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicOr(volatile atomicord32 *paoDestination, atomicord32 aoBitMask)
{
	register atomicord32 aoResult;
	register atomicord32 aoExchange;

	asm volatile (
	"0:;"
		"movl  %4, %2;"
		"orl   %3, %2;"
		"lock; cmpxchgl %2, %0;"
		"jnz   0;"
		: "=m" (*(volatile _ou_atomic_CLargeStruct *)paoDestination), "=a" (aoResult), "=r" (aoExchange)
		: "a" (*paoDestination), "g" (aoBitMask), "m" (*paoDestination)
		: "memory");

	return aoResult;
}

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicXor(volatile atomicord32 *paoDestination, atomicord32 aoBitMask)
{
	register atomicord32 aoResult;
	register atomicord32 aoExchange;

	asm volatile (
	"0:;"
		"movl  %4, %2;"
		"xorl  %3, %2;"
		"lock; cmpxchgl %2, %0;"
		"jnz   0;"
		: "=m" (*(volatile _ou_atomic_CLargeStruct *)paoDestination), "=a" (aoResult), "=r" (aoExchange)
		: "a" (*paoDestination), "g" (aoBitMask), "m" (*paoDestination)
		: "memory");

	return aoResult;
}


#endif // #if defined(_OU_ATOMIC_USE_X86_ASSEMBLER)


#endif // #if _OU_TARGET_OS == _OU_TARGET_OS_GENUNIX


//////////////////////////////////////////////////////////////////////////
// BitMask to CompareExchange forwarders

#if defined(__OU_ATOMIC_ORD32_FUNCTIONS_DEFINED) && !defined(__OU_ATOMIC_BIT_FUNCTIONS_DEFINED)

#define __OU_ATOMIC_BIT_FUNCTIONS_DEFINED

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicAnd(volatile atomicord32 *paoDestination, atomicord32 aoBitMask)
{
	atomicord32 aoOldValue = *paoDestination;
	
    while (true)
	{
		if (AtomicCompareExchange(paoDestination, aoOldValue, (aoOldValue & aoBitMask)))
		{
			break;
		}
		
		aoOldValue = *paoDestination;
    }
	
    return aoOldValue;
}

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicOr(volatile atomicord32 *paoDestination, atomicord32 aoBitMask)
{
	atomicord32 aoOldValue = *paoDestination;
	
    while (true)
	{
		if (AtomicCompareExchange(paoDestination, aoOldValue, (aoOldValue | aoBitMask)))
		{
			break;
		}
		
		aoOldValue = *paoDestination;
    }
	
    return aoOldValue;
}

static _OU_ALWAYSINLINE_PRE atomicord32 _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicord32 */AtomicXor(volatile atomicord32 *paoDestination, atomicord32 aoBitMask)
{
	atomicord32 aoOldValue = *paoDestination;
	
    while (true)
	{
		if (AtomicCompareExchange(paoDestination, aoOldValue, (aoOldValue ^ aoBitMask)))
		{
			break;
		}
		
		aoOldValue = *paoDestination;
    }
	
    return aoOldValue;
}


#endif // #if defined(__OU_ATOMIC_ORD32_FUNCTIONS_DEFINED) && !defined(__OU_ATOMIC_BIT_FUNCTIONS_DEFINED)


//////////////////////////////////////////////////////////////////////////
// Pointer to ord32 forwarders

#if defined(__OU_ATOMIC_ORD32_FUNCTIONS_DEFINED) && !defined(__OU_ATOMIC_PTR_FUNCTIONS_DEFINED) && _OU_TARGET_BITS == _OU_TARGET_BITS_32

#define __OU_ATOMIC_PTR_FUNCTIONS_DEFINED

static _OU_ALWAYSINLINE_PRE atomicptr _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*atomicptr */AtomicExchangePointer(volatile atomicptr *papDestination, atomicptr apExchange)
{
	return (atomicptr)AtomicExchange((volatile atomicord32 *)papDestination, (atomicord32)apExchange);
}

static _OU_ALWAYSINLINE_PRE bool _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*bool */AtomicCompareExchangePointer(volatile atomicptr *papDestination, atomicptr apComparand, atomicptr apExchange)
{
	return AtomicCompareExchange((volatile atomicord32 *)papDestination, (atomicord32)apComparand, (atomicord32)apExchange);
}


#endif // #if defined(__OU_ATOMIC_ORD32_FUNCTIONS_DEFINED) && !defined(__OU_ATOMIC_PTR_FUNCTIONS_DEFINED) && _OU_TARGET_BITS == _OU_TARGET_BITS_32


//////////////////////////////////////////////////////////////////////////
// Atomic-via-mutex implementations

#if !defined(__OU_ATOMIC_ORD32_FUNCTIONS_DEFINED)


END_NAMESPACE_OU();


#include <stddef.h>


BEGIN_NAMESPACE_OU();


typedef int32_t atomicord32;
typedef void *atomicptr;


atomicord32 _OU_CONVENTION_API AtomicIncrement(volatile atomicord32 *paoDestination);
atomicord32 _OU_CONVENTION_API AtomicDecrement(volatile atomicord32 *paoDestination);

atomicord32 _OU_CONVENTION_API AtomicExchange(volatile atomicord32 *paoDestination, atomicord32 aoExchange);
atomicord32 _OU_CONVENTION_API AtomicExchangeAdd(volatile atomicord32 *paoDestination, atomicord32 aoAddend);
bool _OU_CONVENTION_API AtomicCompareExchange(volatile atomicord32 *paoDestination, atomicord32 aoComparand, atomicord32 aoExchange);

atomicord32 _OU_CONVENTION_API AtomicAnd(volatile atomicord32 *paoDestination, atomicord32 aoBitMask);
atomicord32 _OU_CONVENTION_API AtomicOr(volatile atomicord32 *paoDestination, atomicord32 aoBitMask);
atomicord32 _OU_CONVENTION_API AtomicXor(volatile atomicord32 *paoDestination, atomicord32 aoBitMask);


#if defined(__OU_ATOMIC_BIT_FUNCTIONS_DEFINED)

#error Internal error (__OU_ATOMIC_BIT_FUNCTIONS_DEFINED must not be defined in this case)


#endif // #if defined(__OU_ATOMIC_BIT_FUNCTIONS_DEFINED)

#if defined(__OU_ATOMIC_PTR_FUNCTIONS_DEFINED)

#error Internal error (__OU_ATOMIC_PTR_FUNCTIONS_DEFINED must not be defined in this case)


#endif // #if defined(__OU_ATOMIC_PTR_FUNCTIONS_DEFINED)


#endif // #if !defined(__OU_ATOMIC_ORD32_FUNCTIONS_DEFINED)


#if !defined(__OU_ATOMIC_PTR_FUNCTIONS_DEFINED)

atomicptr _OU_CONVENTION_API AtomicExchangePointer(volatile atomicptr *papDestination, atomicptr apExchange);
bool _OU_CONVENTION_API AtomicCompareExchangePointer(volatile atomicptr *papDestination, atomicptr apComparand, atomicptr apExchange);


#define __OU_ATOMIC_OPERATIONS_VIA_MUTEXES
#define __OU_ATOMIC_INITIALIZATION_FUNCTIONS_REQUIRED

// Initialization must be performed from main thread
bool _OU_CONVENTION_API InitializeAtomicAPI();
void _OU_CONVENTION_API FinalizeAtomicAPI();


#endif // #if !defined(__OU_ATOMIC_PTR_FUNCTIONS_DEFINED)


//////////////////////////////////////////////////////////////////////////
// No-result to result forwarders

#if !defined(__OU_ATOMIC_ORD32_NORESULT_FUNCTIONS_DEFINED)

static _OU_ALWAYSINLINE_PRE void _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*void */AtomicIncrementNoResult(volatile atomicord32 *paoDestination)
{
	AtomicIncrement(paoDestination);
}

static _OU_ALWAYSINLINE_PRE void _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*void */AtomicDecrementNoResult(volatile atomicord32 *paoDestination)
{
	AtomicDecrement(paoDestination);
}

static _OU_ALWAYSINLINE_PRE void _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*void */AtomicExchangeAddNoResult(volatile atomicord32 *paoDestination, atomicord32 aoAddend)
{
	AtomicExchangeAdd(paoDestination, aoAddend);
}

static _OU_ALWAYSINLINE_PRE void _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*void */AtomicAndNoResult(volatile atomicord32 *paoDestination, atomicord32 aoBitMask)
{
	AtomicAnd(paoDestination, aoBitMask);
}

static _OU_ALWAYSINLINE_PRE void _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*void */AtomicOrNoResult(volatile atomicord32 *paoDestination, atomicord32 aoBitMask)
{
	AtomicOr(paoDestination, aoBitMask);
}

static _OU_ALWAYSINLINE_PRE void _OU_ALWAYSINLINE_IN _OU_CONVENTION_API 
/*void */AtomicXorNoResult(volatile atomicord32 *paoDestination, atomicord32 aoBitMask)
{
	AtomicXor(paoDestination, aoBitMask);
}


#endif // #if !defined(__OU_ATOMIC_ORD32_NORESULT_FUNCTIONS_DEFINED)


//////////////////////////////////////////////////////////////////////////
// Atomic initialization function stubs

#if !defined(__OU_ATOMIC_INITIALIZATION_FUNCTIONS_REQUIRED)

// Initialization must be performed from main thread
static _OU_INLINE bool _OU_CONVENTION_API InitializeAtomicAPI()
{
	// Do nothing
	
	return true;
}

static _OU_INLINE void _OU_CONVENTION_API FinalizeAtomicAPI()
{
	// Do nothing
}


#endif // #if !defined(__OU_ATOMIC_INITIALIZE_FUNCTIONS_DEFINED)


END_NAMESPACE_OU();


#endif // #ifndef __OU_ATOMIC_H_INCLUDED
