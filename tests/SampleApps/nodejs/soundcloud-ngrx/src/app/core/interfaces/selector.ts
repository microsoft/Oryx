import { Observable } from 'rxjs/Observable';


export type Selector<T,V> = (observable$: Observable<T>) => Observable<V>;
