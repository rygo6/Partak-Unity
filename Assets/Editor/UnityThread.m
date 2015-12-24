//
//  UnityThread.m
//  Unity-iPhone
//
//  Created by Ryan Goodrich on 12/23/15.
//
//

#import <mach/mach.h>
#include <pthread.h>

void SetMoveThreadPriority()
{
    char name[256];
    mach_msg_type_number_t count;
    thread_act_array_t list;
    task_threads(mach_task_self(), &list, &count);
    for (int i = 0; i < count; ++i)
    {
        pthread_t pt = pthread_from_mach_thread_np(list[i]);

        if (pt)
        {
            name[0] = '\0';
            int rc = pthread_getname_np(pt, name, sizeof name);
            NSLog(@"mach thread %u: getname returned %d: %s", list[i], rc, name);
            NSString *threadName = [[NSString alloc] initWithUTF8String:name];
            if ([threadName containsString:@"CellParticleMove"])
            {
                int policy;
                struct sched_param param;
                memset(&param, 0, sizeof(struct sched_param));
                pthread_getschedparam(pt, &policy, &param);
                NSLog(@"%d %d", policy, param.sched_priority);
                param.sched_priority = sched_get_priority_max(policy);
                NSLog(@"%d %d", SCHED_FIFO, param.sched_priority);
                int error = pthread_setschedparam(pt, SCHED_FIFO, &param);
                NSLog(@"CellParticleMove Set %d", error);
            }
        }
        else
        {
            NSLog(@"mach thread %u: no pthread found", list[i]);
        }
    }
}

void SetGradientThreadPriority()
{
    char name[256];
    mach_msg_type_number_t count;
    thread_act_array_t list;
    task_threads(mach_task_self(), &list, &count);
    for (int i = 0; i < count; ++i)
    {
        pthread_t pt = pthread_from_mach_thread_np(list[i]);
        
        if (pt)
        {
            name[0] = '\0';
            int rc = pthread_getname_np(pt, name, sizeof name);
            NSLog(@"mach thread %u: getname returned %d: %s", list[i], rc, name);
            NSString *threadName = [[NSString alloc] initWithUTF8String:name];
            if ([threadName containsString:@"CellGradient"])
            {
                int policy;
                struct sched_param param;
                memset(&param, 0, sizeof(struct sched_param));
                pthread_getschedparam(pt, &policy, &param);
                NSLog(@"%d %d", policy, param.sched_priority);
                param.sched_priority = sched_get_priority_min(policy);
                NSLog(@"%d %d", SCHED_FIFO, param.sched_priority);
                int error = pthread_setschedparam(pt, SCHED_FIFO, &param);
                NSLog(@"CellGradient Set %d", error);
            }
        }
        else
        {
            NSLog(@"mach thread %u: no pthread found", list[i]);
        }
    }
}
