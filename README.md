#KSP Threading System

*****

Multithreading utility for KSP plugins, intended to provide automatic synchronization between plugin multithreaded tasks and the main KSP thread.

Taks can be queued up to be executed within a particular Unity frame (FixedUpdate, Update, LateUpdate) or set up to start during one frame and return the results during the next.
Tasks can also be associated with pre- and post-functions run on the main Unity thread to call Unity functions that are not thread-safe.

Greater documentation can be found at the github wiki: https://github.com/ferram4/KSPTS/wiki