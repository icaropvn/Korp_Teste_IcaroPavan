import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ProdutosAddDialogComponent } from './produtos-add-dialog.component';

describe('ProdutosAddDialogComponent', () => {
  let component: ProdutosAddDialogComponent;
  let fixture: ComponentFixture<ProdutosAddDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProdutosAddDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProdutosAddDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
