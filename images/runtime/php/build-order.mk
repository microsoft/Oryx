# Start the longest translation units early, using only configured JIT objects.
.PHONY: oryx-runtime
oryx-runtime: Zend/zend_execute.lo \
	$(filter ext/opcache/jit/zend_jit.lo ext/opcache/jit/ir/ir.lo ext/opcache/jit/ir/ir_emit.lo,$(PHP_GLOBAL_OBJS) $(shared_objects_opcache)) \
	all
