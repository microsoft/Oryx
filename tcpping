#!/bin/sh
#
# tcpping: test response times using TCP SYN packets
#          URL: http://www.vdberg.org/~richard/tcpping.html
#
# uses tcptraceroute from http://michael.toren.net/code/tcptraceroute/
#
# (c) 2002-2005 Richard van den Berg <richard@vdberg.org> under the GPL
#               http://www.gnu.org/copyleft/gpl.html
#
# 2002/12/20 v1.0 initial version
# 2003/01/25 v1.1 added -c and -r options
#                 now accepting all other tcptraceroute options
# 2003/01/30 v1.2 removed double quotes around backquotes
# 2003/03/25 v1.3 added -x option, courtesy of Alvin Austin <alvin@crlogic.com>
# 2005/03/31 v1.4 added -C option, courtesy of Norman Rasmussen <norman@rasmussen.org>
# 2007/01/11 v1.5 catch bad destination addresses
# 2007/01/19 v1.6 catch non-root tcptraceroute
# 2008/02/10 v1.7 make -C work when reverse lookup fails, courtesy of Fabrice Le Dorze <Fabrice.LeDorze@apx.fr>


ver="v1.7"
format="%Y%m%d%H%M%S"
d="no"
c="no"
C="no"
ttl=255
seq=0
q=1
r=1
w=3
topts=""

usage () {
	name=`basename $0`
	echo "tcpping $ver Richard van den Berg <richard@vdberg.org>"
	echo
	echo "Usage: $name [-d] [-c] [-C] [-w sec] [-q num] [-x count] ipaddress [port]"
	echo
	echo "        -d   print timestamp before every result"
	echo "        -c   print a columned result line"
	echo "        -C   print in the same format as fping's -C option"
	echo "        -w   wait time in seconds (defaults to 3)"
	echo "        -r   repeat every n seconds (defaults to 1)"
	echo "        -x   repeat n times (defaults to unlimited)"
	echo
	echo "See also: man tcptraceroute"
	echo
}

_checksite() {
	ttr=`tcptraceroute -f ${ttl} -m ${ttl} -q ${q} -w ${w} $* 2>&1`
	if echo "${ttr}" | egrep -i "(bad destination|got roo)" >/dev/null 2>&1; then
		echo "${ttr}"
		exit
	fi
}
	
_testsite() {
	myseq="${1}"
	shift
	[ "${c}" = "yes" ] && nows=`date +${format}`
	[ "${d}" = "yes" ] && nowd=`date`
	ttr=`tcptraceroute -f ${ttl} -m ${ttl} -q ${q} -w ${w} $* 2>/dev/null`
	host=`echo "${ttr}" | awk '{print $2 " " $3}'`
	rtt=`echo "${ttr}" | sed 's/.*] //' | awk '{print $1}'`
	not=`echo "${rtt}" | tr -d ".0123456789"`
	[ "${d}" = "yes" ] && echo "$nowd"
	if [ "${c}" = "yes" ]; then
		if [ "x${rtt}" != "x" -a "x${not}" = "x" ]; then
			echo "$myseq $nows $rtt $host"
		else
			echo "$myseq $nows $max $host"
		fi
	elif [ "${C}" = "yes" ]; then
		if [ "$myseq" = "0" ]; then
			echo -n "$1 :"
		fi
		if [ "x${rtt}" != "x" -a "x${not}" = "x" ]; then
			echo -n " $rtt"
		else
			echo -n " -"
		fi
		if [ "$x" = "1" ]; then
			echo
		fi
	else
		echo "${ttr}" | sed -e "s/^.*\*.*$/seq $myseq: no response (timeout)/" -e "s/^$ttl /seq $myseq: tcp response from/"
	fi
#       echo "${ttr}"
}

while getopts dhq:w:cr:nNFSAEi:f:l:m:p:s:x:C opt ; do
	case "$opt" in
		d|c|C) eval $opt="yes" ;;
		q|w|r|x) eval $opt="$OPTARG" ;;
		n|N|F|S|A|E) topt="$topt -$opt" ;;
		i|l|p|s) topt="$topt -$opt $OPTARG" ;;
		f|m) ttl="$OPTARG" ;;
		?) usage; exit ;;
	esac
done

shift `expr $OPTIND - 1`

if [ "x$1" = "x" ]; then
	usage
	exit
fi

max=`echo "${w} * 1000" | bc`

if [ `date +%s` != "%s" ]; then
	format="%s"
fi

_checksite ${topt} $*

if [ "$x" = "" ]; then
	while [ 1 ] ; do
		_testsite ${seq} ${topt} $* &
		pid=$!
		if [ "${C}" = "yes" ]; then
			wait $pid
		fi
		seq=`expr $seq + 1`
		sleep ${r}
	done
else
	while [ "$x" -gt 0 ] ; do
		_testsite ${seq} ${topt} $* &
		pid=$!
		if [ "${C}" = "yes" ]; then
			wait $pid
		fi
		seq=`expr $seq + 1`
		x=`expr $x - 1`
		if [ "$x" -gt 0 ]; then
			sleep ${r}
		fi
	done
fi

exit

