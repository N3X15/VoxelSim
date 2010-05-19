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

#ifndef __OU_ASSERT_H_INCLUDED
#define __OU_ASSERT_H_INCLUDED


#include <ou/customization.h>
#include <ou/namespace.h>


/*
 *	Implementation Note:
 *	1) Fully qualified names must be used in macros as they might be 
 *	used externally and forwarded from outside of _OU_NAMESPACE.
 *	2) false || ... is necessary to suppress C4800 warning in MSVC.
 */


#if defined(NDEBUG)

#define OU_ASSERT(Condition) ((void)0)

#define OU_VERIFY(Condition) ((void)(Condition))

#define OU_CHECK(Condition) (void)(false || (Condition) || (!_OU_NAMESPACE::CAssertionCheckCustomization::GetAssertFailureCustomHandler() || (_OU_NAMESPACE::CAssertionCheckCustomization::GetAssertFailureCustomHandler()(_OU_NAMESPACE::AFS_CHECK,  #Condition, __FILE__, __LINE__), false)) || (*(int *)0 = 0))


#else // #if !defined(NDEBUG)

#include <assert.h>


#define OU__ASSERT_HANDLER(Condition) (false || (Condition) || (_OU_NAMESPACE::CAssertionCheckCustomization::GetAssertFailureCustomHandler() && (_OU_NAMESPACE::CAssertionCheckCustomization::GetAssertFailureCustomHandler()(_OU_NAMESPACE::AFS_ASSERT, #Condition, __FILE__, __LINE__), true)))

#define OU__CHECK_HANDLER(Condition) ((bConditionValue = false || (Condition)) || (_OU_NAMESPACE::CAssertionCheckCustomization::GetAssertFailureCustomHandler() && (_OU_NAMESPACE::CAssertionCheckCustomization::GetAssertFailureCustomHandler()(_OU_NAMESPACE::AFS_CHECK,  #Condition, __FILE__, __LINE__), true)))


#define OU_ASSERT(Condition) assert(OU__ASSERT_HANDLER(Condition))

#define OU_VERIFY(Condition) OU_ASSERT(Condition)

#define OU_CHECK(Condition) { \
	bool bConditionValue; \
	assert(OU__CHECK_HANDLER(Condition)); \
	(void)(bConditionValue || (*(int *)0 = 0)); \
}


#endif // #if !defined(NDEBUG)



#endif // #ifndef __OU_ASSERT_H_INCLUDED
