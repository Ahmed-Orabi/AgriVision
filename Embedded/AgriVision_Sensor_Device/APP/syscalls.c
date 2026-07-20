/**
 * @file syscalls.c
 * @brief Minimal newlib syscall stubs for bare-metal linking.
 *
 * The firmware does not use files or an operating-system console. These stubs
 * satisfy newlib symbols and keep the linker output clean when using
 * --specs=nosys.specs.
 */

#include <sys/stat.h>

int _close(int file)
{
    (void)file;
    return -1;
}

int _fstat(int file, struct stat *status)
{
    (void)file;

    if (status != 0)
    {
        status->st_mode = S_IFCHR;
    }

    return 0;
}

int _getpid(void)
{
    return 1;
}

int _isatty(int file)
{
    (void)file;
    return 1;
}

int _kill(int pid, int sig)
{
    (void)pid;
    (void)sig;
    return -1;
}

int _lseek(int file, int ptr, int dir)
{
    (void)file;
    (void)ptr;
    (void)dir;
    return 0;
}

int _read(int file, char *ptr, int len)
{
    (void)file;
    (void)ptr;
    (void)len;
    return 0;
}

int _write(int file, char *ptr, int len)
{
    (void)file;
    (void)ptr;
    return len;
}
