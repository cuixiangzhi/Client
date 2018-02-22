// var vm = require('vm'),
//  code = 'var square = n * n;',
//  script = vm.createScript(code),
//  sandbox;
// var ctx = vm.createContext(sandbox);
// benchmark('vm.runInThisContext',   function() { vm.runInThisContext(code); });
// benchmark('vm.runInNewContext',   function() { vm.runInNewContext(code, sandbox); });
// benchmark('script.runInThisContext', function() { script.runInThisContext(); });
// benchmark('script.runInNewContext', function() { script.runInNewContext(sandbox); });
// benchmark('script.runInContext', function() { script.runInContext(ctx); });
// benchmark('fn',           function() { fn(n); });
var fn = new Function('n', "function testFun(){ console.log('dynamicLog')}");
//# sourceMappingURL=DynamicFun.js.map